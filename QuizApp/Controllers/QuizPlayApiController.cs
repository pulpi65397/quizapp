using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuizApp.Data;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizPlayApiController : ControllerBase
    {
        private readonly QuizAppContext _context;

        public QuizPlayApiController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: api/QuizPlayApi/Start/5?userId=1
        [HttpGet("Start/{id}")]
        public async Task<IActionResult> Start(int id, [FromQuery] string userId)
        {
            var quiz = await _context.Quiz
                .Include(q => q.Pytania)
                    .ThenInclude(p => p.Odpowiedzi)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound(new { Message = "Quiz not found." });
            }

            return Ok(new { Quiz = quiz, UserId = userId });
        }

        // POST: api/QuizPlayApi/CheckAnswer
        [HttpPost("CheckAnswer")]
        public IActionResult CheckAnswer([FromBody] CheckAnswerModel model)
        {
            var pytanie = _context.Pytanie
                .Include(p => p.Odpowiedzi)
                .FirstOrDefault(p => p.Id == model.PytanieId && p.QuizId == model.QuizId);

            if (pytanie == null)
            {
                return NotFound(new { Message = "Question not found." });
            }

            var poprawnaOdpowiedz = pytanie.Odpowiedzi.Any(o => o.Id == model.OdpowiedzId && o.CzyPoprawna);
            int punktyZaOdpowiedz = poprawnaOdpowiedz ? (int)(1000 * (1 - (double)model.CzasOdpowiedzi / 30000) ): 0;

            return Ok(new { Poprawna = poprawnaOdpowiedz, Punkty = punktyZaOdpowiedz });
        }

        // POST: api/QuizPlayApi/Submit
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitQuizModel model)
        {
            int punkty = 0;
            var czasyOdpowiedzi = new Dictionary<int, long>();
            var odpowiedzi = new Dictionary<int, int>(); // Nowy słownik dla odpowiedzi

            foreach (var odpowiedz in model.Odpowiedzi)
            {
                var pytanie = await _context.Pytanie
                    .Include(p => p.Odpowiedzi)
                    .FirstOrDefaultAsync(p => p.Id == odpowiedz.Key);

                if (pytanie != null)
                {
                    var odpowiedzId = odpowiedz.Value[0];
                    var czasOdpowiedzi = odpowiedz.Value[1];

                    // Zbierz odpowiedzi i czasy
                    odpowiedzi.Add(odpowiedz.Key, odpowiedzId);
                    czasyOdpowiedzi.Add(odpowiedz.Key, czasOdpowiedzi);

                    // Oblicz punkty
                    var poprawnaOdpowiedz = pytanie.Odpowiedzi
                        .FirstOrDefault(o => o.Id == odpowiedzId && o.CzyPoprawna);

                    if (poprawnaOdpowiedz != null)
                    {
                        punkty += (int)(1000 * (1 - (double)czasOdpowiedzi / 30000));
                    }
                }
            }

            var wynik = new Wynik
            {
                QuizId = model.QuizId,
                UzytkownikId = model.UserId,
                Punkty = punkty,
                OdpowiedziJson = JsonConvert.SerializeObject(odpowiedzi),
                CzasyOdpowiedziJson = JsonConvert.SerializeObject(czasyOdpowiedzi)
            };

            _context.Wynik.Add(wynik);
            await _context.SaveChangesAsync();

            return Ok(new { Punkty = punkty });
        }
    }

    public class CheckAnswerModel
    {
        public int QuizId { get; set; }
        public int PytanieId { get; set; }
        public int OdpowiedzId { get; set; }
        public long CzasOdpowiedzi { get; set; }
    }

    public class SubmitQuizModel
    {
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public Dictionary<int, int[]> Odpowiedzi { get; set; }
    }
}