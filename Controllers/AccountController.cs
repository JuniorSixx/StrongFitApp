using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrongFitApp.Data;
using StrongFitApp.Models;
using System.Linq;
using System.Security.Claims;
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

        [AllowAnonymous]
        [HttpPost] // ✅ Correção aqui — era HttpGet!
        public IActionResult ExternalLogin(string provider, string returnUrl = "/")
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Erro externo: {remoteError}");
                return RedirectToAction("Login", "Account");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser { UserName = email, Email = email };
                await _userManager.CreateAsync(user);
                await _userManager.AddLoginAsync(user, info);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }
    }
}
