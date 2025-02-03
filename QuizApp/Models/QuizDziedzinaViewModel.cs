using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace QuizApp.Models
{
    public class QuizDziedzinaViewModel
    {
        public List<Quiz>? Quizy { get; set; }
        public SelectList? Dziedziny { get; set; }
        public string? QuizDziedzina { get; set; }
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
    }
}