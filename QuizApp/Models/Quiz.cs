namespace QuizApp.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Tytul { get; set; }
        public int CzasTrwania { get; set; } // Czas trwania w sekundach
        public ICollection<Pytanie> Pytania { get; set; }
    }
}
