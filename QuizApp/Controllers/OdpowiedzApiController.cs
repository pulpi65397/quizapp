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
    public class OdpowiedzApiController : ControllerBase
    {
        private readonly QuizAppContext _context;

        public OdpowiedzApiController(QuizAppContext context)
        {
            _context = context;
        }

        // GET: api/OdpowiedzApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Odpowiedz>>> GetOdpowiedzi()
        {
            return await _context.Odpowiedz.ToListAsync();
        }

        // GET: api/OdpowiedzApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Odpowiedz>> GetOdpowiedz(int id)
        {
            var odpowiedz = await _context.Odpowiedz.FindAsync(id);

            if (odpowiedz == null)
            {
                return NotFound();
            }

            return odpowiedz;
        }

        // POST: api/OdpowiedzApi
        [HttpPost]
        public async Task<ActionResult<Odpowiedz>> PostOdpowiedz(Odpowiedz odpowiedz)
        {
            _context.Odpowiedz.Add(odpowiedz);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOdpowiedz", new { id = odpowiedz.Id }, odpowiedz);
        }

        // PUT: api/OdpowiedzApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOdpowiedz(int id, Odpowiedz odpowiedz)
        {
            if (id != odpowiedz.Id)
            {
                return BadRequest();
            }

            _context.Entry(odpowiedz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OdpowiedzExists(id))
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

        // DELETE: api/OdpowiedzApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOdpowiedz(int id)
        {
            var odpowiedz = await _context.Odpowiedz.FindAsync(id);
            if (odpowiedz == null)
            {
                return NotFound();
            }

            _context.Odpowiedz.Remove(odpowiedz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OdpowiedzExists(int id)
        {
            return _context.Odpowiedz.Any(e => e.Id == id);
        }
    }
}