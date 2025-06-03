using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using StrongFitApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;

namespace StrongFitApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly StrongFitContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            StrongFitContext context,
            UserManager<IdentityUser> userManager = null,
            RoleManager<IdentityRole> roleManager = null)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeedDatabase()
        {
            try
            {
                // Chamar o método de inicialização do banco de dados
                await DbInitializer.InitializeDatabaseAsync(_context);
                TempData["Message"] = "Banco de dados inicializado com sucesso!";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = $"Erro ao inicializar o banco de dados: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // view SeedData
        [HttpGet]
        public IActionResult SeedData()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeedDataConfirmed()
        {
            try
            {
                if (_userManager == null || _roleManager == null)
                {
                    TempData["Error"] = "Não foi possível acessar os serviços de gerenciamento de usuários.";
                    return RedirectToAction("Index", "Home");
                }

                await DbInitializer.InitializeAsync(_context, _userManager, _roleManager);

                TempData["Message"] = "Banco de dados populado com sucesso! Conta de admin criada.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao popular o banco de dados: {ex.Message}";
                if (ex.InnerException != null)
                {
                    TempData["Error"] += $" | Erro interno: {ex.InnerException.Message}";
                }
                return RedirectToAction("SeedData");
            }
        }
    }
}
