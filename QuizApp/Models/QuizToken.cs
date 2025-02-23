namespace QuizApp.Models
{
    public class QuizToken
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string Token { get; set; }
        public DateTime ExpirationDate { get; set; } // Opcjonalnie: data wygaśnięcia tokenu

        public Quiz Quiz { get; set; }
    }
}
