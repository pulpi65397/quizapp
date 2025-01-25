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
    public class PytanieController : Controller
    {
        private readonly QuizAppContext _context;

        public PytanieController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: Pytanie
        public async Task<IActionResult> Index()
        {
              return _context.Pytanie != null ? 
                          View(await _context.Pytanie.ToListAsync()) :
                          Problem("Entity set 'QuizAppContext.Pytanie'  is null.");
        }

        // GET: Pytanie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pytanie == null)
            {
                return NotFound();
            }

            var pytanie = await _context.Pytanie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pytanie == null)
            {
                return NotFound();
            }

            return View(pytanie);
        }

        // GET: Pytanie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pytanie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tekst,PoprawnaOdpowiedz")] Pytanie pytanie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pytanie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pytanie);
        }

        // GET: Pytanie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pytanie == null)
            {
                return NotFound();
            }

            var pytanie = await _context.Pytanie.FindAsync(id);
            if (pytanie == null)
            {
                return NotFound();
            }
            return View(pytanie);
        }

        // POST: Pytanie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tekst,PoprawnaOdpowiedz")] Pytanie pytanie)
        {
            if (id != pytanie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pytanie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PytanieExists(pytanie.Id))
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
            return View(pytanie);
        }

        // GET: Pytanie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pytanie == null)
            {
                return NotFound();
            }

            var pytanie = await _context.Pytanie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pytanie == null)
            {
                return NotFound();
            }

            return View(pytanie);
        }

        // POST: Pytanie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pytanie == null)
            {
                return Problem("Entity set 'QuizAppContext.Pytanie'  is null.");
            }
            var pytanie = await _context.Pytanie.FindAsync(id);
            if (pytanie != null)
            {
                _context.Pytanie.Remove(pytanie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PytanieExists(int id)
        {
          return (_context.Pytanie?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
