using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StrongFitApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System;
using StrongFitApp.Models;

namespace StrongFitApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DiagnosticController : Controller
    {
        private readonly StrongFitContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DiagnosticController(
            StrongFitContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Status()
        {
            var model = new DiagnosticViewModel
            {
                DatabaseStatus = await CheckDatabaseStatus(),
                TablesStatus = await CheckTablesStatus(),
                UsersCount = await _userManager.Users.CountAsync(),
                RolesCount = await _roleManager.Roles.CountAsync(),
                PersonalsCount = await _context.Personals.CountAsync(),
                AlunosCount = await _context.Alunos.CountAsync(),
                ExerciciosCount = await _context.Exercicios.CountAsync(),
                TreinosCount = await _context.Treinos.CountAsync()
            };

            return View(model);
        }

        private async Task<bool> CheckDatabaseStatus()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<TableStatus>> CheckTablesStatus()
        {
            var tables = new List<TableStatus>();

            try
            {
                tables.Add(new TableStatus
                {
                    Name = "AspNetUsers",
                    Exists = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM AspNetUsers") >= 0,
                    RecordsCount = await _userManager.Users.CountAsync()
                });

                tables.Add(new TableStatus
                {
                    Name = "AspNetRoles",
                    Exists = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM AspNetRoles") >= 0,
                    RecordsCount = await _roleManager.Roles.CountAsync()
                });

                tables.Add(new TableStatus
                {
                    Name = "Personals",
                    Exists = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM Personals") >= 0,
                    RecordsCount = await _context.Personals.CountAsync()
                });

                tables.Add(new TableStatus
                {
                    Name = "Alunos",
                    Exists = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM Alunos") >= 0,
                    RecordsCount = await _context.Alunos.CountAsync()
                });

                tables.Add(new TableStatus
                {
                    Name = "Exercicios",
                    Exists = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM Exercicios") >= 0,
                    RecordsCount = await _context.Exercicios.CountAsync()
                });

                tables.Add(new TableStatus
                {
                    Name = "Treinos",
                    Exists = await _context.Database.ExecuteSqlRawAsync("SELECT COUNT(*) FROM Treinos") >= 0,
                    RecordsCount = await _context.Treinos.CountAsync()
                });
            }
            catch (Exception ex)
            {
                tables.Add(new TableStatus
                {
                    Name = "Erro",
                    Exists = false,
                    RecordsCount = 0,
                    ErrorMessage = ex.Message
                });
            }

            return tables;
        }

        [HttpPost]
        public async Task<IActionResult> ResetDatabase()
        {
            try
            {
                // Removem todos os dados existentes
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM TreinoExercicio");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Treinos");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Exercicios");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Alunos");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Personals");

                await DbInitializer.InitializeDatabaseAsync(_context);

                TempData["Message"] = "Banco de dados reinicializado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao reinicializar o banco de dados: {ex.Message}";
            }

            return RedirectToAction("Status");
        }

        [HttpPost]
        public async Task<IActionResult> SeedDatabase()
        {
            try
            {
                // banco de dados com exemplo
                await DbInitializer.InitializeDatabaseAsync(_context);

                TempData["Message"] = "Dados de exemplo adicionados com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao adicionar dados de exemplo: {ex.Message}";
            }

            return RedirectToAction("Status");
        }

        [HttpGet]
        public async Task<IActionResult> TestRegisterAluno()
        {
            try
            {
                var personal = await _context.Personals.FirstOrDefaultAsync();
                if (personal == null)
                {
                    TempData["Error"] = "Não existe nenhum personal cadastrado. Cadastre um personal primeiro.";
                    return RedirectToAction("Status");
                }

                var testEmail = $"teste_{DateTime.Now.Ticks}@strongfit.com"; //teste de usuari
                var user = new IdentityUser { UserName = testEmail, Email = testEmail, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, "Teste@123");

                if (!result.Succeeded)
                {
                    TempData["Error"] = $"Erro ao criar usuário: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                    return RedirectToAction("Status");
                }

                // Adicionar role Aluno
                if (!await _roleManager.RoleExistsAsync("Aluno"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Aluno"));
                }
                await _userManager.AddToRoleAsync(user, "Aluno");

                // Criar o aluno
                var aluno = new Aluno
                {
                    Nome = "Aluno Teste",
                    Email = testEmail,
                    Data_Nascimento = DateTime.Now.AddYears(-20),
                    Telefone = "(00) 00000-0000",
                    Instagram = "@alunoteste",
                    PersonalID = personal.PersonalID,
                    Observacoes = "Aluno de teste criado pelo diagnóstico"
                };

                _context.Alunos.Add(aluno);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"Aluno de teste criado com sucesso! ID: {aluno.AlunoID}, Email: {testEmail}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao criar aluno de teste: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" | Erro interno: {ex.InnerException.Message}";
                }
            }

            return RedirectToAction("Status");
        }
    }
}
