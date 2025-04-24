using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class Uzytkownik
    {
        public int Id { get; set; }
        [Required]
        public string Nick { get; set; }
        //public int Punkty { get; set; }
        //public string ConnectionId { get; set; } // Dodaj ConnectionId do śledzenia połączenia SignalR
        //public Dictionary<int, int> Odpowiedzi { get; set; } = new(); // PytanieId -> OdpowiedzId
        //public Dictionary<int, long> CzasyOdpowiedzi { get; set; } = new(); // PytanieId -> Czas odpowiedzi (ms)
    }
}