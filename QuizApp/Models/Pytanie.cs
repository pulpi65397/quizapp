﻿using System.ComponentModel.DataAnnotations;
namespace QuizApp.Models
{
    public class Pytanie
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public List<Odpowiedz>? Odpowiedzi { get; set; } = new List<Odpowiedz>();
        
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
    }
}
