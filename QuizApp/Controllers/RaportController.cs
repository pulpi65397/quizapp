using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace QuizApp.Controllers
{
    public class RaportController : Controller
    {
        private readonly QuizAppContext _context;

        public RaportController(QuizAppContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var wyniki = await _context.Wynik
                .Join(
                    _context.Uzytkownik,
                    wynik => wynik.UzytkownikId,
                    uzytkownik => uzytkownik.Id,
                    (wynik, uzytkownik) => new WynikViewModel
                    {
                        QuizId = wynik.QuizId,
                        UzytkownikNick = uzytkownik.Nick,
                        Punkty = wynik.Punkty
                    }
                )
                .ToListAsync();

            return View(wyniki);
        }

        public async Task<IActionResult> EksportujCsv()
        {
            var wyniki = await PobierzDaneDoEksportu();

            using (var writer = new StringWriter())
            {
                writer.WriteLine("QuizId,UzytkownikNick,Punkty");

                foreach (var wynik in wyniki)
                {
                    writer.WriteLine($"{wynik.QuizId},{wynik.UzytkownikNick},{wynik.Punkty}");
                }

                return File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", "Raport.csv");
            }
        }
        private async Task<List<WynikViewModel>> PobierzDaneDoEksportu()
        {
            return await _context.Wynik
                .Join(
                    _context.Uzytkownik,
                    wynik => wynik.UzytkownikId,
                    uzytkownik => uzytkownik.Id,
                    (wynik, uzytkownik) => new WynikViewModel
                    {
                        QuizId = wynik.QuizId,
                        UzytkownikNick = uzytkownik.Nick,
                        Punkty = wynik.Punkty
                    }
                )
                .ToListAsync();
        }
    }
}