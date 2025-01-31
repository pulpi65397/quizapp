using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class OdpowiedzController : Controller
    {
        private readonly QuizAppContext _context;

        public OdpowiedzController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: Odpowiedz
        public async Task<IActionResult> Index()
        {
            var quizAppContext = _context.Odpowiedz.Include(o => o.Pytanie);
            return View(await quizAppContext.ToListAsync());
        }

        // GET: Odpowiedz/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Odpowiedz == null)
            {
                return NotFound();
            }

            var odpowiedz = await _context.Odpowiedz
                .Include(o => o.Pytanie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (odpowiedz == null)
            {
                return NotFound();
            }

            return View(odpowiedz);
        }

        // GET: Odpowiedz/Create
        public IActionResult Create()
        {
            ViewData["PytanieId"] = new SelectList(_context.Pytanie, "Id", "Id");
            return View();
        }

        // POST: Odpowiedz/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tekst,CzyPoprawna,PytanieId")] Odpowiedz odpowiedz)
        {
            ModelState.Remove("Pytanie");
            if (ModelState.IsValid)
            {
                _context.Add(odpowiedz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PytanieId"] = new SelectList(_context.Pytanie, "Id", "Id", odpowiedz.PytanieId);
            return View(odpowiedz);
        }

        // GET: Odpowiedz/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Odpowiedz == null)
            {
                return NotFound();
            }

            var odpowiedz = await _context.Odpowiedz.FindAsync(id);
            if (odpowiedz == null)
            {
                return NotFound();
            }
            ViewData["PytanieId"] = new SelectList(_context.Pytanie, "Id", "Id", odpowiedz.PytanieId);
            return View(odpowiedz);
        }

        // POST: Odpowiedz/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tekst,CzyPoprawna,PytanieId")] Odpowiedz odpowiedz)
        {
            if (id != odpowiedz.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Pojazd");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(odpowiedz);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OdpowiedzExists(odpowiedz.Id))
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
            ViewData["PytanieId"] = new SelectList(_context.Pytanie, "Id", "Id", odpowiedz.PytanieId);
            return View(odpowiedz);
        }

        // GET: Odpowiedz/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Odpowiedz == null)
            {
                return NotFound();
            }

            var odpowiedz = await _context.Odpowiedz
                .Include(o => o.Pytanie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (odpowiedz == null)
            {
                return NotFound();
            }

            return View(odpowiedz);
        }

        // POST: Odpowiedz/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Odpowiedz == null)
            {
                return Problem("Entity set 'QuizAppContext.Odpowiedz'  is null.");
            }
            var odpowiedz = await _context.Odpowiedz.FindAsync(id);
            if (odpowiedz != null)
            {
                _context.Odpowiedz.Remove(odpowiedz);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OdpowiedzExists(int id)
        {
          return (_context.Odpowiedz?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
