using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace QuizApp.Models
{
    public class Wynik
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UzytkownikId { get; set; }
        public int Punkty { get; set; }
        public TimeSpan CzasUkonczenia { get; set; }
        public string OdpowiedziJson { get; set; } // Zmieniono na string
        public string CzasyOdpowiedziJson { get; set; } // Zmieniono na string
        public bool Poprawna { get; set; }

        public Quiz Quiz { get; set; }
        public Uzytkownik Uzytkownik { get; set; }
    }
}