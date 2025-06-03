using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StrongFitApp.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager,
                          UserManager<IdentityUser> userManager,
                          ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório")]
            [EmailAddress(ErrorMessage = "Email inválido")]
            public string Email { get; set; }

            [Required(ErrorMessage = "A senha é obrigatória")]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; }

            [Display(Name = "Lembrar-me")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Verificar se o usuário existe antes de tentar fazer login
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    return Page();
                }

                // Tentar fazer login
                var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuário logado com sucesso.");

                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (roles.Contains("Personal"))
                    {
                        return RedirectToAction("Index", "Alunos");
                    }
                    else if (roles.Contains("Aluno"))
                    {
                        return RedirectToAction("MeusTreinos", "Treinos");
                    }

                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Conta de usuário bloqueada.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tentativa de login inválida. Verifique seu email e senha.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
