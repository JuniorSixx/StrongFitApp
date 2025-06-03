using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StrongFitApp.Data;
using StrongFitApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace StrongFitApp.Controllers
{
    [Authorize(Roles = "Admin, Personal")]
    public class AlunosController : Controller
    {
        private readonly StrongFitContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AlunosController(StrongFitContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Admin"))
            {
                // Admin ve todos os alunos
                var alunos = await _context.Alunos
                    .Include(a => a.Personal)
                    .ToListAsync();
                return View(alunos);
            }
            else if (User.IsInRole("Personal"))
            {
                // Personal ve apenas seus alunos
                var personal = await _context.Personals
                    .FirstOrDefaultAsync(p => p.Email == currentUser.Email);

                if (personal == null)
                {
                    return NotFound();
                }

                var alunos = await _context.Alunos
                    .Include(a => a.Personal)
                    .Where(a => a.PersonalID == personal.PersonalID)
                    .ToListAsync();
                return View(alunos);
            }

            return Forbid();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Alunos == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos
                .Include(a => a.Personal)
                .Include(a => a.Treinos)
                    .ThenInclude(t => t.Exercicios)
                .FirstOrDefaultAsync(m => m.AlunoID == id);

            if (aluno == null)
            {
                return NotFound();
            }

            // personal atual tem acesso a este aluno
            if (User.IsInRole("Personal") && !await VerificarAcessoAoAluno(aluno.AlunoID))
            {
                return Forbid();
            }

            return View(aluno);
        }

        public async Task<IActionResult> Create()
        {
            await CarregarPersonals();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlunoID,Nome,Email,Data_Nascimento,Telefone,Instagram,Observacoes,PersonalID")] Aluno aluno)
        {
            if (ModelState.IsValid)
            {
                if (User.IsInRole("Personal") && !await VerificarAcessoAoPersonal(aluno.PersonalID))
                {
                    return Forbid();
                }

                _context.Add(aluno);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await CarregarPersonals();
            return View(aluno);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Alunos == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Personal") && !await VerificarAcessoAoAluno(aluno.AlunoID))
            {
                return Forbid();
            }

            await CarregarPersonals();
            return View(aluno);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AlunoID,Nome,Email,Data_Nascimento,Telefone,Instagram,Observacoes,PersonalID")] Aluno aluno)
        {
            if (id != aluno.AlunoID)
            {
                return NotFound();
            }

            if (User.IsInRole("Personal") && !await VerificarAcessoAoAluno(aluno.AlunoID))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aluno);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlunoExists(aluno.AlunoID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            await CarregarPersonals();
            return View(aluno);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Alunos == null)
            {
                return NotFound();
            }

            var aluno = await _context.Alunos
                .Include(a => a.Personal)
                .FirstOrDefaultAsync(m => m.AlunoID == id);

            if (aluno == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Personal") && !await VerificarAcessoAoAluno(aluno.AlunoID))
            {
                return Forbid();
            }

            return View(aluno);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Alunos == null)
            {
                return Problem("Entity set 'StrongFitContext.Alunos'  is null.");
            }

            var aluno = await _context.Alunos.FindAsync(id);
            if (aluno == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Personal") && !await VerificarAcessoAoAluno(aluno.AlunoID))
            {
                return Forbid();
            }

            _context.Alunos.Remove(aluno);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlunoExists(int id)
        {
            return (_context.Alunos?.Any(e => e.AlunoID == id)).GetValueOrDefault();
        }

        private async Task CarregarPersonals()
        {
            if (User.IsInRole("Admin"))
            {
                ViewData["PersonalID"] = new SelectList(await _context.Personals.ToListAsync(), "PersonalID", "Nome");
            }
            else if (User.IsInRole("Personal"))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var personal = await _context.Personals.FirstOrDefaultAsync(p => p.Email == currentUser.Email);

                if (personal != null)
                {
                    var personals = new List<Personal> { personal };
                    ViewData["PersonalID"] = new SelectList(personals, "PersonalID", "Nome");
                }
            }
        }

        private async Task<bool> VerificarAcessoAoAluno(int alunoID)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var personal = await _context.Personals.FirstOrDefaultAsync(p => p.Email == currentUser.Email);

            if (personal == null)
            {
                return false;
            }

            var aluno = await _context.Alunos.FindAsync(alunoID);
            return aluno != null && aluno.PersonalID == personal.PersonalID;
        }

        private async Task<bool> VerificarAcessoAoPersonal(int personalID)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var personal = await _context.Personals.FirstOrDefaultAsync(p => p.Email == currentUser.Email);

            if (personal == null)
            {
                return false;
            }

            return personal.PersonalID == personalID;
        }
    }
}
