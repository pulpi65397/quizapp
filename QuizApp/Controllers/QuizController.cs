using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using Microsoft.Extensions.Logging;

namespace QuizApp.Controllers
{

    public class QuizController : Controller
    {
        private readonly QuizAppContext _context;
        private readonly ILogger<QuizController> _logger;

        public QuizController(QuizAppContext context, ILogger<QuizController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            _logger.LogInformation("### Rozpoczęto akcję Edit GET dla id: {Id}", id);

            if (id == null)
            {
                _logger.LogWarning("### Brak ID w żądaniu Edit GET");
                return NotFound();
            }

            try
            {
                var quiz = await _context.Quiz
                    .Include(q => q.Pytania)
                        .ThenInclude(p => p.Odpowiedzi)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null)
                {
                    _logger.LogWarning("### Nie znaleziono quizu o ID: {Id}", id);
                    return NotFound();
                }

                _logger.LogDebug("### Znaleziono quiz: {@Quiz}", quiz);
                return View(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "### Błąd podczas pobierania quizu do edycji");
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            
            for (int i = 0; i < quiz.Pytania.Count; i++)
            {
                var selectedAnswer = Request.Form[$"Pytania[{i}].SelectedAnswer"].FirstOrDefault();
                if (selectedAnswer != null && int.TryParse(selectedAnswer, out int selectedIndex))
                {
                    for (int j = 0; j < quiz.Pytania[i].Odpowiedzi.Count; j++)
                    {
                        quiz.Pytania[i].Odpowiedzi[j].CzyPoprawna = (j == selectedIndex);
                    }
                }
            }

            
            if (quiz.Pytania != null)
            {
                foreach (var pytanie in quiz.Pytania)
                {
                    if (pytanie.Odpowiedzi?.Any(o => o.CzyPoprawna) != true)
                    {
                        ModelState.AddModelError("", $"Pytanie '{pytanie.Tekst}' musi mieć dokładnie jedną poprawną odpowiedź.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    var existingQuiz = await _context.Quiz
                        .Include(q => q.Pytania)
                            .ThenInclude(p => p.Odpowiedzi)
                        .FirstOrDefaultAsync(q => q.Id == id);

                    if (existingQuiz == null)
                    {
                        return NotFound();
                    }

                    
                    existingQuiz.Tytul = quiz.Tytul;
                    existingQuiz.LiczbaPytan = quiz.LiczbaPytan;
                    existingQuiz.Dziedzina = quiz.Dziedzina;

                    
                    var existingPytania = existingQuiz.Pytania.ToDictionary(p => p.Id);

                    foreach (var newPytanie in quiz.Pytania)
                    {
                        if (newPytanie.Id > 0 && existingPytania.TryGetValue(newPytanie.Id, out var existingPytanie))
                        {
                            
                            existingPytanie.Tekst = newPytanie.Tekst;

                            
                            var existingOdpowiedzi = existingPytanie.Odpowiedzi.ToDictionary(o => o.Id);
                            foreach (var newOdpowiedz in newPytanie.Odpowiedzi)
                            {
                                if (newOdpowiedz.Id > 0 && existingOdpowiedzi.TryGetValue(newOdpowiedz.Id, out var existingOdp))
                                {
                                    existingOdp.Tekst = newOdpowiedz.Tekst;
                                    existingOdp.CzyPoprawna = newOdpowiedz.CzyPoprawna; 
                                }
                                else if (newOdpowiedz.Id == 0)
                                {
                                    existingPytanie.Odpowiedzi.Add(new Odpowiedz
                                    {
                                        Tekst = newOdpowiedz.Tekst,
                                        CzyPoprawna = newOdpowiedz.CzyPoprawna
                                    });
                                }
                            }
                        }
                        else if (newPytanie.Id == 0)
                        {
                            existingQuiz.Pytania.Add(new Pytanie
                            {
                                Tekst = newPytanie.Tekst,
                                Odpowiedzi = newPytanie.Odpowiedzi.Select(o => new Odpowiedz
                                {
                                    Tekst = o.Tekst,
                                    CzyPoprawna = o.CzyPoprawna
                                }).ToList()
                            });
                        }
                    }

                   
                    var receivedIds = quiz.Pytania.Select(p => p.Id).ToHashSet();
                    foreach (var pytanie in existingQuiz.Pytania.ToList())
                    {
                        if (!receivedIds.Contains(pytanie.Id) && pytanie.Id != 0)
                        {
                            _context.Pytanie.Remove(pytanie);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Błąd podczas edycji quizu");
                    ModelState.AddModelError("", "Nie można zapisać zmian. Sprawdź poprawność danych.");
                }
            }

            
            return View(quiz);
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

            
            if (!string.IsNullOrEmpty(searchString))
            {
                quizy = quizy.Where(q => EF.Functions.Like(q.Tytul, "%" + searchString + "%") ||
                                            EF.Functions.Like(q.Dziedzina, "%" + searchString + "%"));
            }

            
            if (!string.IsNullOrEmpty(quizDziedzina))
            {
                quizy = quizy.Where(x => x.Dziedzina == quizDziedzina);
            }

            
            ViewData["TytulSortParm"] = String.IsNullOrEmpty(sortOrder) ? "tytul_desc" : "";
            ViewData["CzasTrwaniaSortParm"] = sortOrder == "czastrwania" ? "czastrwania_desc" : "czastrwania";
            ViewData["DziedzinaSortParm"] = sortOrder == "dziedzina" ? "dziedzina_desc" : "dziedzina";

            switch (sortOrder)
            {
                case "tytul_desc":
                    quizy = quizy.OrderByDescending(s => s.Tytul);
                    break;
                case "liczbapytan":
                    quizy = quizy.OrderBy(s => s.LiczbaPytan);
                    break;
                case "liczbapytan_desc":
                    quizy = quizy.OrderByDescending(s => s.LiczbaPytan);
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
        public async Task<IActionResult> Create([Bind("Tytul,LiczbaPytan,Dziedzina,Pytania")] Quiz quiz)
        {
            
            foreach (var pytanie in quiz.Pytania)
            {
                var selectedAnswerIndex = Request.Form[$"Pytania[{pytanie.Id}].SelectedAnswer"].FirstOrDefault();
                if (selectedAnswerIndex != null && int.TryParse(selectedAnswerIndex, out int index))
                {
                    for (int i = 0; i < pytanie.Odpowiedzi.Count; i++)
                    {
                        pytanie.Odpowiedzi[i].CzyPoprawna = (i == index);
                    }
                }
            }

            
            if (quiz.Pytania != null)
            {
                foreach (var pytanie in quiz.Pytania)
                {
                    if (pytanie.Odpowiedzi?.Any(o => o.CzyPoprawna) != true)
                    {
                        ModelState.AddModelError("", $"Pytanie '{pytanie.Tekst}' musi mieć co najmniej jedną poprawną odpowiedź.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(quiz);
        }

        // Akcja Delete (GET) - wyświetla potwierdzenie usunięcia quizu
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

        // Akcja Delete (POST) - usuwa quiz z bazy danych
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

        // Metoda pomocnicza - sprawdza, czy quiz o danym ID istnieje
        private bool QuizExists(int id)
        {
            return (_context.Quiz?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Akcja GenerateToken - generuje token dla quizu
        public JsonResult GenerateToken(int quizId)
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

            return Json(new { token = token });
        }

        // Akcja Token - wyświetla token quizu
        public IActionResult Token(string token)
        {
            return View("Token", token);
        }
    }
}