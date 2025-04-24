using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaportApiController : ControllerBase
    {
        private readonly QuizAppContext _context;

        public RaportApiController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: api/RaportApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WynikViewModel>>> GetRaport()
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

            return Ok(wyniki);
        }

        // GET: api/RaportApi/ExportCsv
        [HttpGet("ExportCsv")]
        public async Task<IActionResult> ExportCsv()
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

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("QuizId,UzytkownikNick,Punkty");

            foreach (var wynik in wyniki)
            {
                csv.AppendLine($"{wynik.QuizId},{wynik.UzytkownikNick},{wynik.Punkty}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Raport.csv");
        }
    }
}