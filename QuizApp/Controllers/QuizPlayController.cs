using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System;

namespace QuizApp.Controllers
{
    public class QuizPlayController : Controller
    {
        private readonly QuizAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuizPlayController(QuizAppContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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
            var rankingViewModel = new RankingViewModel { Uzytkownicy = new List<RankingUzytkownika>() };

            // Przekazujemy model quizu do widoku głównego, a pusty model rankingu do Partial View
            ViewData["RankingViewModel"] = rankingViewModel; // Alternatywnie, możesz użyć ViewBag

            return View(quiz);
        }

        [HttpPost]
        public async Task<IActionResult> Sprawdz(int quizId, int pytanieId, int odpowiedzId, long czasOdpowiedzi)
        {
            var pytanie = await _context.Pytanie
                .Include(p => p.Odpowiedzi)
                .FirstOrDefaultAsync(p => p.Id == pytanieId && p.QuizId == quizId);

            if (pytanie == null)
            {
                return NotFound();
            }

            var poprawnaOdpowiedz = pytanie.Odpowiedzi.Any(o => o.Id == odpowiedzId && o.CzyPoprawna);

            int punktyZaOdpowiedz = 0;
            if (poprawnaOdpowiedz)
            {
                int maxPunkty = 1000;
                long maxCzas = 30000;

                punktyZaOdpowiedz = (int)(maxPunkty * (1 - (double)czasOdpowiedzi / maxCzas));
                if (punktyZaOdpowiedz < 0) punktyZaOdpowiedz = 0;
            }

            return Json(new { poprawna = poprawnaOdpowiedz, punkty = punktyZaOdpowiedz });
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int quizId, Dictionary<int, Tuple<int, long>> odpowiedzi)
        {
            int punkty = 0;
            foreach (var odpowiedz in odpowiedzi)
            {
                var pytanie = await _context.Pytanie
                    .Include(p => p.Odpowiedzi)
                    .FirstOrDefaultAsync(p => p.Id == odpowiedz.Key && p.QuizId == quizId);

                if (pytanie != null)
                {
                    var poprawnaOdpowiedz = pytanie.Odpowiedzi
                        .FirstOrDefault(o => o.Id == odpowiedz.Value.Item1 && o.CzyPoprawna);

                    if (poprawnaOdpowiedz != null)
                    {
                        int maxPunkty = 1000;
                        long maxCzas = 30000;
                        long czasOdpowiedzi = odpowiedz.Value.Item2;

                        punkty += (int)(maxPunkty * (1 - (double)czasOdpowiedzi / maxCzas));
                        if (punkty < 0) punkty = 0;
                    }
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                // Obsługa gościa -  alternatywnie można użyć anonimowych identyfikatorów
                var guestUser = await _userManager.FindByNameAsync("guest");

                if (guestUser == null)
                {
                    guestUser = new ApplicationUser { UserName = "guest", Nick = "guest" };
                    var result = await _userManager.CreateAsync(guestUser, "Haslo123"); // Zmień na silne hasło w produkcji!
                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }
                }
                userId = guestUser.Id;
            }


            var wynik = new Wynik
            {
                QuizId = quizId,
                UzytkownikId = userId,
                Punkty = punkty,
                CzasUkonczenia = TimeSpan.Zero // Dodaj logikę zapisu czasu ukończenia quizu
            };

            _context.Wynik.Add(wynik);
            await _context.SaveChangesAsync();

            return RedirectToAction("Wynik", new { quizId = quizId, punkty = punkty });
        }


        public async Task<IActionResult> Wynik(int quizId, int punkty)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                var guestUser = await _userManager.FindByNameAsync("guest");
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

            var wynik = await _context.Wynik
                .FirstOrDefaultAsync(w => w.QuizId == quizId && w.UzytkownikId == userId);

            ViewBag.Punkty = punkty;

            return View("~/Views/Wynik/Index.cshtml");
        }


    }
}