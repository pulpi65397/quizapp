namespace QuizApp.Models
{
    public class Wynik
    {
        public int Id { get; set; }
        public int UzytkownikId { get; set; }
        public Uzytkownik Uzytkownik { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int Punkty { get; set; }
    }
}
