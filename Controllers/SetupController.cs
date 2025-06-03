using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using StrongFitApp.Data;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace StrongFitApp.Controllers
{
    public class SetupController : Controller
    {
        private readonly StrongFitContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SetupController(
            StrongFitContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Initialize()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initialize(string confirm)
        {
            if (confirm != "confirm")
            {
                ModelState.AddModelError(string.Empty, "Por favor, confirme a inicialização digitando 'confirm'.");
                return View();
            }

            try
            {
                // checa se existe um usuário admin
                var adminExists = await _userManager.Users.AnyAsync(u => u.Email == "admin@strongfit.com");

                if (adminExists)
                {
                    TempData["Message"] = "A conta de administrador já existe. Você pode fazer login com admin@strongfit.com.";
                    return RedirectToAction("Index", "Home");
                }

                string[] roleNames = { "Admin", "Personal", "Aluno" };
                foreach (var roleName in roleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                var adminUser = new IdentityUser
                {
                    UserName = "admin@strongfit.com",
                    Email = "admin@strongfit.com",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");

                    // Inicia osdados 
                    await DbInitializer.InitializeDatabaseAsync(_context);

                    TempData["Message"] = "Sistema inicializado com sucesso! Você pode fazer login com admin@strongfit.com e senha Admin@123";
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["Error"] = $"Erro ao criar usuário admin: {errors}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro durante a inicialização: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" | Erro interno: {ex.InnerException.Message}";
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
