namespace QuizApp.Models
{
    public class Uzytkownik
    {
        public int Id { get; set; }
        public string Nick { get; set; }
        public string Email { get; set; }
        public ICollection<Wynik> Wyniki { get; set; }

    }
}
