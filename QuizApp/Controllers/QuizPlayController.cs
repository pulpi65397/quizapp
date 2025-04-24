using Microsoft.AspNetCore.Mvc;
using QuizApp.Data;
using QuizApp.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

public class QuizPlayController : Controller
{
    private readonly QuizAppContext _context;

    public QuizPlayController(QuizAppContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Start(int? id)
    {
        var userId = Request.Query["userId"].ToString();
        ViewBag.UserId = userId;
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
        ViewData["RankingViewModel"] = rankingViewModel;

        return View(quiz);
    }

    [HttpPost]
    public JsonResult Sprawdz(int quizId, int pytanieId, int odpowiedzId, long czasOdpowiedzi)
    {
        var pytanie = _context.Pytanie
            .Include(p => p.Odpowiedzi)
            .FirstOrDefault(p => p.Id == pytanieId && p.QuizId == quizId);

        if (pytanie == null)
        {
            return Json(new Wynik { Poprawna = false, Punkty = 0 });
        }

        var poprawnaOdpowiedz = pytanie.Odpowiedzi.Any(o => o.Id == odpowiedzId && o.CzyPoprawna);

        int punktyZaOdpowiedz = 0;
        if (poprawnaOdpowiedz)
        {
            int maxPunkty = 1000;
            long maxCzas = 30000;

            punktyZaOdpowiedz = maxPunkty - (int)(maxPunkty * Math.Min(1, (double)czasOdpowiedzi / 1000));
            if (punktyZaOdpowiedz < 0) punktyZaOdpowiedz = 0;
        }

        var wynikSprawdzenia = new Wynik { Poprawna = poprawnaOdpowiedz, Punkty = punktyZaOdpowiedz };
        return Json(wynikSprawdzenia);
    }

    [HttpPost]
    public async Task<IActionResult> Submit(int quizId, Dictionary<int, Tuple<int, long>> odpowiedzi, string userNick)
    {
        int punkty = 0;
        int userId;

        
        var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(loggedUserId) && int.TryParse(loggedUserId, out userId))
        {
            // Użytkownik jest zalogowany, użyj jego ID
        }
        else
        {
            
            var existingUzytkownik = await _context.Uzytkownik
                .FirstOrDefaultAsync(u => u.Nick == userNick);

            if (existingUzytkownik == null)
            {
                var newUzytkownik = new Uzytkownik
                {
                    Nick = userNick
                };
                _context.Uzytkownik.Add(newUzytkownik);
                await _context.SaveChangesAsync();
                userId = newUzytkownik.Id; 
            }
            else
            {
                userId = existingUzytkownik.Id; 
            }
        }


        foreach (var odpowiedz in odpowiedzi)
        {
            var pytanie = await _context.Pytanie
                .Include(p => p.Odpowiedzi)
                .FirstOrDefaultAsync(p => p.Id == odpowiedz.Key);

            if (pytanie != null)
            {
                var poprawnaOdpowiedz = pytanie.Odpowiedzi
                    .FirstOrDefault(o => o.Id == odpowiedz.Value.Item1 && o.CzyPoprawna);

                if (poprawnaOdpowiedz != null)
                {
                    punkty += (int)(1000 * (odpowiedz.Value.Item2 <= 1000 ? 1 : (1 - (double)odpowiedz.Value.Item2 / 30000)));
                }
            }
        }


        var wynik = new Wynik
        {
            QuizId = quizId,
            UzytkownikId = userId, 
            Punkty = punkty
        };

        _context.Wynik.Add(wynik);
        await _context.SaveChangesAsync();

        return Ok();
    }

    public async Task<IActionResult> Wynik(int quizId, int punkty, string userId)
    {
        int parsedUserId;
        Uzytkownik uzytkownik;

        
        var loggedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(loggedUserId) && int.TryParse(loggedUserId, out parsedUserId))
        {
            
            uzytkownik = await _context.Uzytkownik.FindAsync(parsedUserId);
            if (uzytkownik == null)
            {
                return NotFound("Nie znaleziono użytkownika w bazie danych.");
            }
        }
        else
        {
            
            if (!int.TryParse(userId, out parsedUserId))
            {
                return BadRequest("Nieprawidłowy identyfikator użytkownika.");
            }

            
            uzytkownik = await _context.Uzytkownik.FindAsync(parsedUserId);
            if (uzytkownik == null)
            {
                return NotFound("Nie znaleziono użytkownika-goscia.");
            }
        }

        
        var wynik = new Wynik
        {
            QuizId = quizId,
            UzytkownikId = parsedUserId,
            Punkty = punkty,
            CzasyOdpowiedziJson = "[]",
            OdpowiedziJson = "[]"
        };

        
        _context.Wynik.Add(wynik);
        await _context.SaveChangesAsync();

        
        ViewBag.Punkty = punkty;

        return View("~/Views/Wynik/Index.cshtml", wynik);
    }

    public async Task<IActionResult> GetUserNick(int userId)
    {
        var uzytkownik = await _context.Uzytkownik.FindAsync(userId);
        if (uzytkownik == null)
        {
            return Content("Gość"); 
        }
        return Content(uzytkownik.Nick);
    }
}