using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace QuizApp.Controllers
{
    public class QuizPlayController : Controller
    {
        private readonly QuizAppContext _context;

        public QuizPlayController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: QuizPlay/Start/5
        public async Task<IActionResult> Start(int? id)
        {
            if (id == null)
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

        // POST: QuizPlay/Submit
        [HttpPost]
        public async Task<IActionResult> Submit(int quizId, Dictionary<int, int> odpowiedzi)
        {
            // Obliczanie punktów na podstawie odpowiedzi użytkownika
            int punkty = await ObliczPunkty(quizId, odpowiedzi);

            // Zapisanie wyniku do bazy danych (bez powiązania z użytkownikiem)
            var wynik = new Wynik
            {
                QuizId = quizId,
                UzytkownikId = null,  // Jeśli nie jest zalogowany, wynik nie będzie przypisany do konkretnego użytkownika
                Punkty = punkty
            };

            _context.Wynik.Add(wynik);
            await _context.SaveChangesAsync();

            // Przekierowanie do akcji "Index" w kontrolerze "Wynik" z quizId i punktami
            return RedirectToAction("Index", "Wynik", new { quizId = quizId, punkty = punkty });
        }

        // Metoda obliczająca punkty
        public async Task<int> ObliczPunkty(int quizId, Dictionary<int, int> odpowiedzi)
        {
            int punkty = 0;

            // Pobranie pytań i odpowiedzi z bazy danych
            var pytania = await _context.Pytanie
                .Include(p => p.Odpowiedzi)
                .Where(p => p.QuizId == quizId)
                .ToListAsync();

            // Sprawdzanie odpowiedzi użytkownika
            foreach (var odpowiedz in odpowiedzi)
            {
                var pytanie = pytania.FirstOrDefault(p => p.Id == odpowiedz.Key);
                if (pytanie != null)
                {
                    // Znajdź odpowiedź na pytanie
                    var poprawnaOdpowiedz = pytanie.Odpowiedzi
                        .FirstOrDefault(o => o.Id == odpowiedz.Value && o.CzyPoprawna);

                    if (poprawnaOdpowiedz != null)
                    {
                        punkty++; // Jeśli odpowiedź jest poprawna, dodaj punkt
                    }
                }
            }

            return punkty;
        }

        // GET: QuizPlay/Wynik/5
        public async Task<IActionResult> Wynik(int quizId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Jeżeli użytkownik nie jest zalogowany, przypisz "guest"
            if (string.IsNullOrEmpty(userId))
            {
                // Sprawdzenie, czy użytkownik "guest" istnieje
                var guestUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "guest");

                // Jeżeli nie istnieje, utwórz go
                if (guestUser == null)
                {
                    guestUser = new ApplicationUser
                    {
                        UserName = "guest",
                        Nick = "guest"
                    };
                    _context.Users.Add(guestUser);
                    await _context.SaveChangesAsync();
                }

                userId = guestUser.Id;
            }

            // Pobierz wynik użytkownika dla danego quizu
            var wynik = await _context.Wynik
                .FirstOrDefaultAsync(w => w.QuizId == quizId && w.UzytkownikId == userId);

            if (wynik == null)
            {
                // Jeżeli wynik nie istnieje, wyświetl widok BrakWyniku
                return View("BrakWyniku");
            }

            // Zwróć widok z wynikiem, który znajduje się w folderze Views/Wynik/Index.cshtml
            return View("~/Views/Wynik/Index.cshtml", wynik);
        }
    }
}
