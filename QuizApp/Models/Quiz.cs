﻿namespace QuizApp.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Tytul { get; set; }
        public int CzasTrwania { get; set; }
        public List<Pytanie> Pytania { get; set; } = new List<Pytanie>();
        public string Dziedzina { get; set; } // Dodana właściwość
    }
}