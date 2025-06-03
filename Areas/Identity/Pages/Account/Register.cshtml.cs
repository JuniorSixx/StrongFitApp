using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StrongFitApp.Data;
using StrongFitApp.Models;

namespace StrongFitApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly StrongFitContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager,
            StrongFitContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string ReturnUrl { get; set; } = string.Empty;

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "A senha é obrigatória")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não correspondem.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "O tipo de usuário é obrigatório")]
            [Display(Name = "Tipo de Usuário")]
            public string UserType { get; set; } = string.Empty;

            [Required(ErrorMessage = "O nome é obrigatório")]
            [Display(Name = "Nome")]
            public string Nome { get; set; } = string.Empty;

            [Display(Name = "Especialidade")]
            public string? Especialidade { get; set; }

            [Display(Name = "Data de Nascimento")]
            [DataType(DataType.Date)]
            public DateTime? DataNascimento { get; set; }

            [Display(Name = "Telefone")]
            public string? Telefone { get; set; }

            [Display(Name = "Instagram")]
            public string? Instagram { get; set; }

            [Display(Name = "Personal")]
            public int? PersonalID { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            try
            {
                await CarregarPersonals();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar personals");
                ModelState.AddModelError(string.Empty, "Erro ao carregar a lista de personals. Verifique se o banco de dados está configurado corretamente.");
            }
        }

        private async Task CarregarPersonals()
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    var personals = await _context.Personals.ToListAsync();
                    ViewData["PersonalID"] = new SelectList(personals, "PersonalID", "Nome");
                    _logger.LogInformation("Personals carregados com sucesso: {Count}", personals.Count);
                }
                else
                {
                    _logger.LogWarning("Não foi possível conectar ao banco de dados");
                    ViewData["PersonalID"] = new SelectList(new List<Personal>(), "PersonalID", "Nome");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar personals");
                ViewData["PersonalID"] = new SelectList(new List<Personal>(), "PersonalID", "Nome");
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            try
            {
                _logger.LogInformation("Iniciando processo de registro para: {Email}, Tipo: {UserType}", Input.Email, Input.UserType);

                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

                await CarregarPersonals();

                if (Input.UserType == "Aluno" && !Input.PersonalID.HasValue)
                {
                    ModelState.AddModelError("Input.PersonalID", "É necessário selecionar um Personal.");
                    _logger.LogWarning("Tentativa de registro de aluno sem personal selecionado");
                    return Page();
                }

                if (ModelState.IsValid)
                {
                    _logger.LogInformation("ModelState válido, prosseguindo com o registro");

                    // Verificar o email
                    var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Input.Email", "Este email já está em uso.");
                        _logger.LogWarning("Email já em uso: {Email}", Input.Email);
                        return Page();
                    }

                    var user = new IdentityUser { UserName = Input.Email, Email = Input.Email, EmailConfirmed = true };
                    var result = await _userManager.CreateAsync(user, Input.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Usuário criado com sucesso: {Email}", Input.Email);

                        if (!await _roleManager.RoleExistsAsync(Input.UserType))
                        {
                            _logger.LogInformation("Criando role: {Role}", Input.UserType);
                            var roleResult = await _roleManager.CreateAsync(new IdentityRole(Input.UserType));
                            if (!roleResult.Succeeded)
                            {
                                foreach (var error in roleResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, $"Erro ao criar role: {error.Description}");
                                    _logger.LogError("Erro ao criar role: {Error}", error.Description);
                                }
                                await _userManager.DeleteAsync(user);
                                return Page();
                            }
                        }

                        // Adicionar usuário
                        _logger.LogInformation("Adicionando usuário à role: {Role}", Input.UserType);
                        var addToRoleResult = await _userManager.AddToRoleAsync(user, Input.UserType);
                        if (!addToRoleResult.Succeeded)
                        {
                            foreach (var error in addToRoleResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, $"Erro ao adicionar à role: {error.Description}");
                                _logger.LogError("Erro ao adicionar à role: {Error}", error.Description);
                            }
                            await _userManager.DeleteAsync(user);
                            return Page();
                        }

                        // Criar entidade correspondente Personal; Aluno;
                        try
                        {
                            if (Input.UserType == "Personal")
                            {
                                _logger.LogInformation("Criando personal: {Nome}", Input.Nome);
                                // Verificar se tem um personal com este email
                                var existingPersonal = await _context.Personals.FirstOrDefaultAsync(p => p.Email == Input.Email);
                                if (existingPersonal != null)
                                {
                                    ModelState.AddModelError("Input.Email", "Já existe um personal com este email.");
                                    _logger.LogWarning("Personal já existe com este email: {Email}", Input.Email);
                                    await _userManager.DeleteAsync(user);
                                    return Page();
                                }

                                var personal = new Personal
                                {
                                    Nome = Input.Nome,
                                    Email = Input.Email,
                                    Especialidade = Input.Especialidade ?? "Geral",
                                    Telefone = Input.Telefone,
                                    Instagram = Input.Instagram
                                };

                                _context.Personals.Add(personal);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Personal criado com sucesso: {ID}", personal.PersonalID);

                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return LocalRedirect("~/Home/Index");
                            }
                            else if (Input.UserType == "Aluno")
                            {
                                _logger.LogInformation("Criando aluno: {Nome}", Input.Nome);
                                // Verificar se tem aluno com o email
                                var existingAluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Email == Input.Email);
                                if (existingAluno != null)
                                {
                                    ModelState.AddModelError("Input.Email", "Já existe um aluno com este email.");
                                    _logger.LogWarning("Aluno já existe com este email: {Email}", Input.Email);
                                    await _userManager.DeleteAsync(user);
                                    return Page();
                                }

                                // Verifica se o personal existe
                                if (Input.PersonalID.HasValue)
                                {
                                    var personal = await _context.Personals.FindAsync(Input.PersonalID.Value);
                                    if (personal == null)
                                    {
                                        ModelState.AddModelError("Input.PersonalID", "O personal selecionado não existe.");
                                        _logger.LogWarning("Personal não encontrado: {ID}", Input.PersonalID.Value);
                                        await _userManager.DeleteAsync(user);
                                        return Page();
                                    }

                                    _logger.LogInformation("Personal encontrado: {ID}", personal.PersonalID);
                                }
                                else
                                {
                                    ModelState.AddModelError("Input.PersonalID", "É necessário selecionar um Personal.");
                                    _logger.LogWarning("Tentativa de registro de aluno sem personal selecionado");
                                    await _userManager.DeleteAsync(user);
                                    return Page();
                                }

                                _logger.LogInformation("Valores para criar aluno: Nome={Nome}, Email={Email}, DataNascimento={DataNascimento}, PersonalID={PersonalID}",
                                    Input.Nome, Input.Email, Input.DataNascimento, Input.PersonalID);

                                var aluno = new Aluno
                                {
                                    Nome = Input.Nome,
                                    Email = Input.Email,
                                    Data_Nascimento = Input.DataNascimento ?? DateTime.Now,
                                    Telefone = Input.Telefone ?? "",
                                    Instagram = Input.Instagram ?? "",
                                    PersonalID = Input.PersonalID!.Value,
                                    Observacoes = ""
                                };

                                _logger.LogInformation("Objeto aluno criado: {Aluno}",
                                    $"Nome={aluno.Nome}, Email={aluno.Email}, Data_Nascimento={aluno.Data_Nascimento}, PersonalID={aluno.PersonalID}");

                                _context.Alunos.Add(aluno);

                                _logger.LogInformation("Salvando aluno no banco de dados...");
                                await _context.SaveChangesAsync();
                                _logger.LogInformation("Aluno criado com sucesso: {ID}", aluno.AlunoID);

                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return LocalRedirect("~/Treinos/MeusTreinos");
                            }
                            else if (Input.UserType == "Admin")
                            {
                                // Para Admin
                                _logger.LogInformation("Criando admin: {Email}", Input.Email);
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                return LocalRedirect("~/Home/Index");
                            }
                        }
                        catch (DbUpdateException dbEx)
                        {
                            _logger.LogError(dbEx, "Erro ao salvar no banco de dados");

                            await _userManager.DeleteAsync(user);

                            if (dbEx.InnerException != null)
                            {
                                ModelState.AddModelError(string.Empty, $"Erro ao salvar no banco de dados: {dbEx.InnerException.Message}");
                                _logger.LogError("Erro interno: {Message}", dbEx.InnerException.Message);
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Erro ao salvar no banco de dados. Verifique se todos os campos estão preenchidos corretamente.");
                            }

                            return Page();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao criar entidade");

                            await _userManager.DeleteAsync(user);

                            ModelState.AddModelError(string.Empty, $"Erro ao criar entidade: {ex.Message}");

                            return Page();
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        _logger.LogError("Erro ao criar usuário: {Error}", error.Description);
                    }
                }
                else
                {
                    _logger.LogWarning("ModelState inválido. Erros: {Errors}",
                        string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário: {Message}", ex.Message);

                // Tentar obter detalhes mais específicos do erro
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError(string.Empty, $"Ocorreu um erro: {ex.InnerException.Message}");
                    _logger.LogError("Erro interno: {Message}", ex.InnerException.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Ocorreu um erro: {ex.Message}");
                }
            }

            return Page();
        }
    }
}
