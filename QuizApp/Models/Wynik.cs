using System.ComponentModel.DataAnnotations;
namespace QuizApp.Models
{
    public class Wynik
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public ApplicationUser Uzytkownik { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int Punkty { get; set; }
    }
}
