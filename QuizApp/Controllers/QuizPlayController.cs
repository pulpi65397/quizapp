using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;

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
                UzytkownikId = 1, // Możesz tu dodać logikę logowania użytkownika
                QuizId = quizId,
                Punkty = punkty
            };

            _context.Add(wynik);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
