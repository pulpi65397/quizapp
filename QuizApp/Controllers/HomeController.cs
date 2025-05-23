﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using System;
using System.Diagnostics;
using System.Linq;

namespace QuizApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QuizAppContext _context;

        public HomeController(ILogger<HomeController> logger, QuizAppContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new JoinQuizViewModel());
        }

        [HttpPost]
        public IActionResult JoinQuiz(JoinQuizViewModel model)
        {
            if (string.IsNullOrEmpty(model.Token))
            {
                ModelState.AddModelError("Token", "Token nie może być pusty.");
                return View("Index", model);
            }

            var quizToken = _context.QuizToken
                .Include(qt => qt.Quiz)
                .FirstOrDefault(qt => qt.Token == model.Token);

            if (quizToken == null)
            {
                ModelState.AddModelError("Token", "Nieprawidłowy token.");
                return View("Index", model);
            }

            if (quizToken.ExpirationDate < DateTime.Now)
            {
                ModelState.AddModelError("Token", "Token wygasł.");
                return View("Index", model);
            }

            model.QuizId = quizToken.QuizId;
            return View("JoinQuizNick", model); // Przekierowanie do widoku z nickiem
        }
        
        [HttpPost]
        public IActionResult JoinQuizWithNick(JoinQuizViewModel model)
        {
            if (string.IsNullOrEmpty(model.Nick))
            {
                ModelState.AddModelError("Nick", "Nick nie może być pusty.");
                return View("JoinQuizNick", model);
            }

            // Zapisz gościa do tabeli Uzytkownik
            var user = new Uzytkownik { Nick = model.Nick };
            _context.Uzytkownik.Add(user);
            _context.SaveChanges();

            // Przekieruj z przekazaniem ID użytkownika
            return Redirect($"/QuizPlay/Start/{model.QuizId}?userId={user.Id}");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}