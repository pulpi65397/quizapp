using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
namespace QuizApp.Models
{
    public class Odpowiedz
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public bool CzyPoprawna { get; set; }

        public int PytanieId { get; set; }
        [ValidateNever]
        [JsonIgnore]
        public Pytanie Pytanie { get; set; }
    }
}
