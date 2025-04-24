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
    public class WynikApiController : ControllerBase
    {
        private readonly QuizAppContext _context;

        public WynikApiController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: api/WynikApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wynik>>> GetWyniki()
        {
            return await _context.Wynik.ToListAsync();
        }

        // GET: api/WynikApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Wynik>> GetWynik(int id)
        {
            var wynik = await _context.Wynik.FindAsync(id);

            if (wynik == null)
            {
                return NotFound();
            }

            return wynik;
        }

        // POST: api/WynikApi
        [HttpPost]
        public async Task<ActionResult<Wynik>> PostWynik(Wynik wynik)
        {
            _context.Wynik.Add(wynik);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWynik", new { id = wynik.Id }, wynik);
        }

        // PUT: api/WynikApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWynik(int id, Wynik wynik)
        {
            if (id != wynik.Id)
            {
                return BadRequest();
            }

            _context.Entry(wynik).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WynikExists(id))
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

        // DELETE: api/WynikApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWynik(int id)
        {
            var wynik = await _context.Wynik.FindAsync(id);
            if (wynik == null)
            {
                return NotFound();
            }

            _context.Wynik.Remove(wynik);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WynikExists(int id)
        {
            return _context.Wynik.Any(e => e.Id == id);
        }
    }
}