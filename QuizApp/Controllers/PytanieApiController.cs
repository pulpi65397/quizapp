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
    public class PytanieApiController : ControllerBase
    {
        private readonly QuizAppContext _context;

        public PytanieApiController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: api/PytanieApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pytanie>>> GetPytania()
        {
            return await _context.Pytanie.ToListAsync();
        }

        // GET: api/PytanieApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pytanie>> GetPytanie(int id)
        {
            var pytanie = await _context.Pytanie.FindAsync(id);

            if (pytanie == null)
            {
                return NotFound();
            }

            return pytanie;
        }

        // POST: api/PytanieApi
        [HttpPost]
        public async Task<ActionResult<Pytanie>> PostPytanie(Pytanie pytanie)
        {
            _context.Pytanie.Add(pytanie);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPytanie", new { id = pytanie.Id }, pytanie);
        }

        // PUT: api/PytanieApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPytanie(int id, Pytanie pytanie)
        {
            if (id != pytanie.Id)
            {
                return BadRequest();
            }

            _context.Entry(pytanie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PytanieExists(id))
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

        // DELETE: api/PytanieApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePytanie(int id)
        {
            var pytanie = await _context.Pytanie.FindAsync(id);
            if (pytanie == null)
            {
                return NotFound();
            }

            _context.Pytanie.Remove(pytanie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PytanieExists(int id)
        {
            return _context.Pytanie.Any(e => e.Id == id);
        }
    }
}