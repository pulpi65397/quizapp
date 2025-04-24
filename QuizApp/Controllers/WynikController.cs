using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System.Security.Claims;

public class WynikController : Controller
{
    private readonly QuizAppContext _context;

    public WynikController(QuizAppContext context)
    {
        _context = context;
    }

    // Akcja do wyświetlania wyników i zapisywania ich do bazy
    public async Task<IActionResult> Wyniki()
    {
        var wyniki = await _context.Wynik
            .Join(
                _context.Uzytkownik, // Używamy własnej tabeli Uzytkownik
                wynik => wynik.UzytkownikId, // Klucz z tabeli Wynik
                uzytkownik => uzytkownik.Id, // Klucz z tabeli Uzytkownik
                (wynik, uzytkownik) => new WynikViewModel
                {
                    QuizId = wynik.QuizId,
                    UzytkownikNick = uzytkownik.Nick, // Zakładając, że nick jest w polu Nick
                    Punkty = wynik.Punkty
                }
            )
            .ToListAsync();

        return View(wyniki);
    }
}