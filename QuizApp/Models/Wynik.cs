using System.ComponentModel.DataAnnotations;
namespace QuizApp.Models
{
    public class Wynik
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int QuizId { get; set; }
        public string? UzytkownikId { get; set; }  // Powinna być nullable

        public Quiz Quiz { get; set; }

        public int Punkty { get; set; }

    }
}
