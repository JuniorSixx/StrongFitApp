using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrongFitApp.Data;
using StrongFitApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StrongFitApp.Controllers
{
    [Authorize(Roles = "Personal,Admin")]
    public class PersonalsController : Controller
    {
        private readonly StrongFitContext _context;

        public PersonalsController(StrongFitContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var personals = await _context.Personals
                .Include(p => p.Alunos)
                .ToListAsync();
            return View(personals);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personal = await _context.Personals
                .Include(p => p.Alunos)
                .FirstOrDefaultAsync(m => m.PersonalID == id);
            if (personal == null)
            {
                return NotFound();
            }

            return View(personal);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PersonalID,Nome,Especialidade")] Personal personal)
        {
            if (ModelState.IsValid)
            {
                _context.Add(personal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(personal);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personal = await _context.Personals.FindAsync(id);
            if (personal == null)
            {
                return NotFound();
            }
            return View(personal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PersonalID,Nome,Especialidade")] Personal personal)
        {
            if (id != personal.PersonalID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(personal);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonalExists(personal.PersonalID))
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
            return View(personal);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personal = await _context.Personals
                .FirstOrDefaultAsync(m => m.PersonalID == id);
            if (personal == null)
            {
                return NotFound();
            }

            return View(personal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personal = await _context.Personals.FindAsync(id);
            if (personal != null)
            {
                _context.Personals.Remove(personal);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PersonalExists(int id)
        {
            return _context.Personals.Any(e => e.PersonalID == id);
        }
    }
}