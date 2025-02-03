using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuizApp.Models
{
    public class Odpowiedz
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public bool CzyPoprawna { get; set; }

        public int PytanieId { get; set; }
        [ValidateNever]
        public Pytanie Pytanie { get; set; }
    }
}
