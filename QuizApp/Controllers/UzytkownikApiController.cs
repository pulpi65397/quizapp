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
    public class UzytkownikApiController : ControllerBase
    {
        private readonly QuizAppContext _context;

        public UzytkownikApiController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: api/UzytkownikApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Uzytkownik>>> GetUzytkownicy()
        {
            return await _context.Uzytkownik.ToListAsync();
        }

        // GET: api/UzytkownikApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Uzytkownik>> GetUzytkownik(int id)
        {
            var uzytkownik = await _context.Uzytkownik.FindAsync(id);

            if (uzytkownik == null)
            {
                return NotFound();
            }

            return uzytkownik;
        }

        // POST: api/UzytkownikApi
        [HttpPost]
        public async Task<ActionResult<Uzytkownik>> PostUzytkownik(Uzytkownik uzytkownik)
        {
            _context.Uzytkownik.Add(uzytkownik);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUzytkownik", new { id = uzytkownik.Id }, uzytkownik);
        }

        // PUT: api/UzytkownikApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUzytkownik(int id, Uzytkownik uzytkownik)
        {
            if (id != uzytkownik.Id)
            {
                return BadRequest();
            }

            _context.Entry(uzytkownik).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UzytkownikExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/UzytkownikApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUzytkownik(int id)
        {
            var uzytkownik = await _context.Uzytkownik.FindAsync(id);
            if (uzytkownik == null)
            {
                return NotFound();
            }

            _context.Uzytkownik.Remove(uzytkownik);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UzytkownikExists(int id)
        {
            return _context.Uzytkownik.Any(e => e.Id == id);
        }
    }
}