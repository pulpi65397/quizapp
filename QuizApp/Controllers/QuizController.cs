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
    public class QuizController : Controller
    {
        private readonly QuizAppContext _context;

        public QuizController(QuizAppContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string quizDziedzina, string searchString, string sortOrder)
        {
            if (_context.Quiz == null)
            {
                return Problem("Entity set 'QuizAppContext.Quiz' is null.");
            }

            IQueryable<string> dziedzinaQuery = from q in _context.Quiz
                                                orderby q.Dziedzina
                                                select q.Dziedzina;

            var quizy = from q in _context.Quiz
                        select q;

            // Wyszukiwanie (z EF.Functions.Like)
            if (!string.IsNullOrEmpty(searchString))
            {
                quizy = quizy.Where(q => EF.Functions.Like(q.Tytul, "%" + searchString + "%") ||
                                            EF.Functions.Like(q.Dziedzina, "%" + searchString + "%"));
            }

            // Filtrowanie po dziedzinie
            if (!string.IsNullOrEmpty(quizDziedzina))
            {
                quizy = quizy.Where(x => x.Dziedzina == quizDziedzina);
            }

            // Sortowanie
            ViewData["TytulSortParm"] = String.IsNullOrEmpty(sortOrder) ? "tytul_desc" : "";
            ViewData["CzasTrwaniaSortParm"] = sortOrder == "czastrwania" ? "czastrwania_desc" : "czastrwania";
            ViewData["DziedzinaSortParm"] = sortOrder == "dziedzina" ? "dziedzina_desc" : "dziedzina";

            switch (sortOrder)
            {
                case "tytul_desc":
                    quizy = quizy.OrderByDescending(s => s.Tytul);
                    break;
                case "czastrwania":
                    quizy = quizy.OrderBy(s => s.CzasTrwania);
                    break;
                case "czastrwania_desc":
                    quizy = quizy.OrderByDescending(s => s.CzasTrwania);
                    break;
                case "dziedzina":
                    quizy = quizy.OrderBy(s => s.Dziedzina);
                    break;
                case "dziedzina_desc":
                    quizy = quizy.OrderByDescending(s => s.Dziedzina);
                    break;
                default:
                    quizy = quizy.OrderBy(s => s.Tytul);
                    break;
            }

            var quizDziedzinaVM = new QuizDziedzinaViewModel
            {
                Dziedziny = new SelectList(await dziedzinaQuery.Distinct().ToListAsync()),
                Quizy = await quizy.ToListAsync(),
                QuizDziedzina = quizDziedzina,
                SearchString = searchString,
                SortOrder = sortOrder
            };

            return View(quizDziedzinaVM);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Quiz == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quiz
                .Include(q => q.Pytania)
                .ThenInclude(p => p.Odpowiedzi)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tytul,CzasTrwania,Dziedzina, Pytania")] Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                var pytania = quiz.Pytania;
                quiz.Pytania = new List<Pytanie>();

                _context.Add(quiz);
                await _context.SaveChangesAsync();

                foreach (var pytanie in pytania)
                {
                    pytanie.QuizId = quiz.Id;
                    _context.Add(pytanie);
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Quiz == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quiz
                .Include(q => q.Pytania)
                .ThenInclude(p => p.Odpowiedzi)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }
            return View(quiz);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tytul,CzasTrwania,Dziedzina")] Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quiz);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quiz.Id))
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
            return View(quiz);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Quiz == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quiz
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Quiz == null)
            {
                return Problem("Entity set 'QuizAppContext.Quiz' is null.");
            }
            var quiz = await _context.Quiz.FindAsync(id);
            if (quiz != null)
            {
                _context.Quiz.Remove(quiz);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuizExists(int id)
        {
            return (_context.Quiz?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Nowa akcja do generowania tokenu
        public JsonResult GenerateToken(int quizId) // Zmiana typu zwracanego na JsonResult
        {
            var random = new Random();
            var token = random.Next(100000, 999999).ToString();

            var quizToken = new QuizToken
            {
                QuizId = quizId,
                Token = token,
                ExpirationDate = DateTime.Now.AddMinutes(30)
            };
            _context.QuizToken.Add(quizToken);
            _context.SaveChanges();

            return Json(new { token = token }); // Poprawne zwracanie JSON
        }

        public IActionResult Token(string token)
        {
            return View("Token", token);
        }
    }
}