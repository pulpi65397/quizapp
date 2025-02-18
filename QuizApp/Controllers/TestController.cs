// Controllers/TestController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data; // Dodaj using do kontekstu bazy danych
using QuizApp.Models; // Dodaj using do modeli
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
    public class TestController : Controller
    {
        private readonly QuizAppContext _context;

        public TestController(QuizAppContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Wyniki()
        {
            var wyniki = await _context.Wynik.ToListAsync(); // Pobierz wszystkie wyniki z bazy danych
            return View(wyniki); // Przekaż wyniki do widoku
        }
    }
}