using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using QuizApp.Data;
using QuizApp.Models;

public class WynikController : Controller
{
    private readonly QuizAppContext _context;

    public WynikController(QuizAppContext context)
    {
        _context = context;
    }

    // Akcja do wyświetlania wyników
    public async Task<IActionResult> Index(int quizId, int punkty)
    {
        // Pobieramy wynik na podstawie quizId (jeśli chcesz wyświetlić inne dane z bazy)
        var wynik = await _context.Wynik
             .FirstOrDefaultAsync(w => w.QuizId == quizId);

        if (wynik == null)
        {
            return NotFound();  // Zwraca 404, jeśli wynik nie został znaleziony
        }

        // Ustawienie punktów, które zostały przekazane
        ViewData["Punkty"] = punkty;

        return View(wynik);  // Wyświetla widok z wynikiem
    }



}
