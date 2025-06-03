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
    [Authorize]
    public class TreinosController : Controller
    {
        private readonly StrongFitContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TreinosController(StrongFitContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Treinos.Include(t => t.Aluno);
            return View(await applicationDbContext.ToListAsync());
        }

        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Treinos == null)
            {
                return NotFound();
            }

            var treino = await _context.Treinos
                .Include(t => t.Aluno)
                .Include(t => t.Exercicios)
                .FirstOrDefaultAsync(m => m.TreinoID == id);
            if (treino == null)
            {
                return NotFound();
            }

            return View(treino);
        }

        [Authorize(Roles = "Admin, Personal")]
        public IActionResult Create()
        {
            ViewData["AlunoID"] = new SelectList(_context.Alunos, "AlunoID", "Nome");
            ViewData["ExerciciosDisponiveis"] = new MultiSelectList(_context.Exercicios, "ExercicioID", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> Create([Bind("TreinoID,AlunoID,Data,Hora,Observacoes")] Treino treino, int[]? selectedExercicios)
        {
            if (ModelState.IsValid)
            {
                _context.Add(treino);
                await _context.SaveChangesAsync();

                if (selectedExercicios != null && selectedExercicios.Length > 0)
                {
                    foreach (var exercicioId in selectedExercicios)
                    {
                        var exercicio = await _context.Exercicios.FindAsync(exercicioId);
                        if (exercicio != null)
                        {
                            if (treino.Exercicios == null)
                            {
                                treino.Exercicios = new List<Exercicio>();
                            }
                            treino.Exercicios.Add(exercicio);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["AlunoID"] = new SelectList(_context.Alunos, "AlunoID", "Nome", treino.AlunoID);
            ViewData["ExerciciosDisponiveis"] = new MultiSelectList(_context.Exercicios, "ExercicioID", "Nome");
            return View(treino);
        }

        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Treinos == null)
            {
                return NotFound();
            }

            var treino = await _context.Treinos
                .Include(t => t.Exercicios)
                .FirstOrDefaultAsync(t => t.TreinoID == id);

            if (treino == null)
            {
                return NotFound();
            }

            ViewData["AlunoID"] = new SelectList(_context.Alunos, "AlunoID", "Nome", treino.AlunoID);

            var exerciciosDisponiveis = await _context.Exercicios.ToListAsync();

            var exerciciosSelecionados = treino.Exercicios != null
                ? treino.Exercicios.Select(e => e.ExercicioID).ToArray()
                : Array.Empty<int>();

            ViewData["ExerciciosDisponiveis"] = new MultiSelectList(
                exerciciosDisponiveis, "ExercicioID", "Nome", exerciciosSelecionados);

            return View(treino);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> Edit(int id, [Bind("TreinoID,AlunoID,Data,Hora,Observacoes")] Treino treino, int[]? selectedExercicios)
        {
            if (id != treino.TreinoID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var treinoExistente = await _context.Treinos
                        .Include(t => t.Exercicios)
                        .FirstOrDefaultAsync(t => t.TreinoID == id);

                    if (treinoExistente == null)
                    {
                        return NotFound();
                    }

                    treinoExistente.AlunoID = treino.AlunoID;
                    treinoExistente.Data = treino.Data;
                    treinoExistente.Hora = treino.Hora;
                    treinoExistente.Observacoes = treino.Observacoes;

                    if (treinoExistente.Exercicios != null)
                    {
                        treinoExistente.Exercicios.Clear();
                    }
                    else
                    {
                        treinoExistente.Exercicios = new List<Exercicio>();
                    }

                    if (selectedExercicios != null && selectedExercicios.Length > 0)
                    {
                        foreach (var exercicioId in selectedExercicios)
                        {
                            var exercicio = await _context.Exercicios.FindAsync(exercicioId);
                            if (exercicio != null)
                            {
                                treinoExistente.Exercicios.Add(exercicio);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreinoExists(treino.TreinoID))
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
            ViewData["AlunoID"] = new SelectList(_context.Alunos, "AlunoID", "Nome", treino.AlunoID);
            ViewData["ExerciciosDisponiveis"] = new MultiSelectList(_context.Exercicios, "ExercicioID", "Nome");
            return View(treino);
        }

        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Treinos == null)
            {
                return NotFound();
            }

            var treino = await _context.Treinos
                .Include(t => t.Aluno)
                .FirstOrDefaultAsync(m => m.TreinoID == id);
            if (treino == null)
            {
                return NotFound();
            }

            return View(treino);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Personal")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Treinos == null)
            {
                return Problem("Entity set 'StrongFitContext.Treinos'  is null.");
            }
            var treino = await _context.Treinos.FindAsync(id);
            if (treino != null)
            {
                _context.Treinos.Remove(treino);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TreinoExists(int id)
        {
            return (_context.Treinos?.Any(e => e.TreinoID == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> MeusTreinos()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var aluno = await _context.Alunos
                .FirstOrDefaultAsync(a => a.Email == user.Email);

            if (aluno == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var treinos = await _context.Treinos
                .Include(t => t.Exercicios)
                .Include(t => t.Aluno)
                .Where(t => t.AlunoID == aluno.AlunoID)
                .OrderByDescending(t => t.Data)
                .ToListAsync();

            return View(treinos);
        }
    }
}
