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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int quizId, List<int> odpowiedzi)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Pytania)
                .ThenInclude(p => p.Odpowiedzi)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }



            // Obliczanie wyników
            int punkty = 0;
            foreach (var pytanie in quiz.Pytania)
            {
                var odpowiedz = odpowiedzi.FirstOrDefault(o => o == pytanie.Id);
                if (odpowiedz != 0)
                {
                    var poprawnaOdpowiedz = pytanie.Odpowiedzi.FirstOrDefault(o => o.CzyPoprawna);
                    if (poprawnaOdpowiedz != null && poprawnaOdpowiedz.Id == odpowiedz)
                    {
                        punkty++;
                    }
                }
            }

            var wynik = new Wynik
            {
                Uzytkownik = await _context.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                QuizId = quizId,
                Punkty = punkty
            };

            _context.Add(wynik);
            await _context.SaveChangesAsync();

            return RedirectToAction("Wynik", new { quizId = quizId }); // Przekierowanie do widoku Wynik
        }
        public async Task<IActionResult> Wynik(int quizId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Pobieramy ID użytkownika z Claims

            if (string.IsNullOrEmpty(userId))
            {
                // Użytkownik nie jest zalogowany
                return RedirectToAction("Login", "Account"); // Przekierowanie do logowania
            }

            var wynik = await _context.Wynik
                .Include(w => w.Uzytkownik)
                .Include(w => w.Quiz)
                .FirstOrDefaultAsync(w => w.QuizId == quizId && w.Uzytkownik.Id == userId); // Używamy właściwości nawigacyjnej Uzytkownik i jego Id

            if (wynik == null)
            {
                // Brak wyniku dla danego quizu i użytkownika
                return View("BrakWyniku"); // Widok informujący o braku wyniku
            }

            return View(wynik);
        }
    }
}
