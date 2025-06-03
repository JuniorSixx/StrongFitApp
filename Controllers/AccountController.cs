using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrongFitApp.Data;
using StrongFitApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StrongFitApp.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly StrongFitContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            StrongFitContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Personal"))
            {
                var personal = await _context.Personals
                    .FirstOrDefaultAsync(p => p.Email == user.Email);

                if (personal != null)
                {
                    return View("PersonalProfile", personal);
                }
            }
            else if (roles.Contains("Aluno"))
            {
                var aluno = await _context.Alunos
                    .Include(a => a.Personal)
                    .FirstOrDefaultAsync(a => a.Email == user.Email);

                if (aluno != null)
                {
                    return View("AlunoProfile", aluno);
                }
            }

            return View("Profile", user);
        }
    }
}