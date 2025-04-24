using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System;

namespace QuizApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeApiController : ControllerBase
    {
        private readonly QuizAppContext _context;

        public HomeApiController(QuizAppContext context)
        {
            _context = context;
        }

        // POST: api/HomeApi/JoinQuiz
        [HttpPost("JoinQuiz")]
        public IActionResult JoinQuiz([FromBody] JoinQuizViewModel model)
        {
            if (string.IsNullOrEmpty(model.Token))
            {
                return BadRequest(new { Message = "Token cannot be empty." });
            }

            var quizToken = _context.QuizToken
                .Include(qt => qt.Quiz)
                .FirstOrDefault(qt => qt.Token == model.Token);

            if (quizToken == null)
            {
                return NotFound(new { Message = "Invalid token." });
            }

            if (quizToken.ExpirationDate < DateTime.Now)
            {
                return BadRequest(new { Message = "Token has expired." });
            }

            return Ok(new { QuizId = quizToken.QuizId });
        }

        // POST: api/HomeApi/JoinQuizWithNick
        [HttpPost("JoinQuizWithNick")]
        public IActionResult JoinQuizWithNick([FromBody] JoinQuizViewModel model)
        {
            if (string.IsNullOrEmpty(model.Nick))
            {
                return BadRequest(new { Message = "Nick cannot be empty." });
            }

            var user = new Uzytkownik { Nick = model.Nick };
            _context.Uzytkownik.Add(user);
            _context.SaveChanges();

            return Ok(new { UserId = user.Id, QuizId = model.QuizId });
        }
    }
}