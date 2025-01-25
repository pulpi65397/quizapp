namespace QuizApp.Models
{
    public class Pytanie
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public string PoprawnaOdpowiedz { get; set; }
        public ICollection<Odpowiedz> Odpowiedzi { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
    }
}
