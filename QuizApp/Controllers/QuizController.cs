using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

public class QuizController : Controller
{
    private readonly QuizAppContext _context;

    public QuizController(QuizAppContext context)
    {
        _context = context;
    }

    // GET: Quizzes
    public async Task<IActionResult> Index()
    {
        var quizzes = await _context.Quiz.ToListAsync();
        return View(quizzes);
    }

    // GET: Quizzes/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var quiz = await _context.Quiz
            .Include(q => q.Pytania) // Eager loading for questions
            .FirstOrDefaultAsync(m => m.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    // GET: Quizzes/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Quizzes/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Tytul,CzasTrwania")] Quiz quiz)
    {
        if (ModelState.IsValid)
        {
            _context.Add(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(quiz);
    }

    // GET: Quizzes/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var quiz = await _context.Quiz.FindAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }
        return View(quiz);
    }

    // POST: Quizzes/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Tytul,CzasTrwania")] Quiz quiz)
    {
        if (id != quiz.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(quiz);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(quiz.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        return View(quiz);
    }

    // GET: Quizzes/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var quiz = await _context.Quiz
            .Include(q => q.Pytania) // Eager loading for questions
            .FirstOrDefaultAsync(m => m.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        return View(quiz);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (_context.Quiz == null)
        {
            return Problem("Entity set 'QuizAppContext.Quiz'  is null.");
        }
        var quiz = await _context.Quiz.FindAsync(id);
        if (quiz != null)
        {
            _context.Quiz.Remove(quiz);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool QuizExists(int id) // Metoda zdefiniowana w klasie QuizController
    {
        return (_context.Quiz?.Any(e => e.Id == id)).GetValueOrDefault();
    }

    public async Task<IActionResult> CreateQuestion(int quizId)
    {
        var quiz = await _context.Quiz.FindAsync(quizId);
        if (quiz == null)
        {
            return NotFound();
        }
        var question = new Pytanie { QuizId = quizId };
        return View(question);
    }

   

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(int quizId, Pytanie pytanie)
    {
        if (ModelState.IsValid)
        {
            var quiz = await _context.Quiz.FindAsync(quizId);
            if (quiz == null)
            {
                return NotFound();
            }
            pytanie.QuizId = quizId; // Ustawienie QuizId w modelu Pytanie
            quiz.Pytania.Add(pytanie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = quizId });
        }
        return View(pytanie);
    }
}