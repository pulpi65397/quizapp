using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace QuizApp.Models
{
    public class Pytanie
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public List<Odpowiedz> Odpowiedzi { get; set; } = new List<Odpowiedz>();
        
        public int QuizId { get; set; }
        [ValidateNever]
        [JsonIgnore]
        public Quiz Quiz { get; set; }
    }
}
