using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using QuizApp.Models;

namespace QuizApp.Hubs
{
    public class QuizHub : Hub
    {
        // Słownik przechowujący stan quizu dla każdego pokoju (quizId)
        private static readonly ConcurrentDictionary<string, QuizState> _quizStates = new();

        public async Task DołączDoQuizu(int quizId, string uzytkownikId, string uzytkownikNick)
        {
            string quizIdStr = quizId.ToString();

            // Dodajemy użytkownika do grupy SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, quizIdStr);

            // Pobieramy lub tworzymy stan quizu
            var quizState = _quizStates.GetOrAdd(quizIdStr, _ => new QuizState());

            // Dodajemy użytkownika do stanu quizu
            quizState.DodajUzytkownika(uzytkownikId, uzytkownikNick, Context.ConnectionId);

            // Wysyłamy informację o dołączeniu użytkownika do wszystkich w grupie
            await Clients.Group(quizIdStr).SendAsync("UzytkownikDolaczyl", uzytkownikId, uzytkownikNick);

            // Wysyłamy aktualną listę użytkowników do nowo dołączonego klienta
            await Clients.Caller.SendAsync("ListaUzytkownikow", quizState.Uzytkownicy);

            // Jeśli quiz jest już w trakcie, wysyłamy aktualny stan do nowego użytkownika
            if (quizState.CzyRozpoczelo)
            {
                await Clients.Caller.SendAsync("RozpocznijQuiz");

                if (quizState.AktualnePytanie != null)
                {
                    await Clients.Caller.SendAsync("PokazPytanie", quizState.AktualnePytanie);

                    // Dodatkowo, jeśli są wyniki, wyślij je do nowego użytkownika
                    if (quizState.Wyniki.ContainsKey(uzytkownikId))
                    {
                        await Clients.Caller.SendAsync("WynikiCzęściowe", quizState.Wyniki[uzytkownikId]);
                    }
                }
            }
        }


        public async Task RozpocznijQuiz(int quizId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                quizState.CzyRozpoczelo = true;
                await Clients.Group(quizIdStr).SendAsync("RozpocznijQuiz");
            }
        }

        public async Task PokazPytanie(int quizId, object pytanie) // 'object' ponieważ przesyłamy serializowany obiekt
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                quizState.AktualnePytanie = pytanie; // Zapisujemy aktualne pytanie w stanie quizu
                await Clients.Group(quizIdStr).SendAsync("PokazPytanie", pytanie);
            }
        }

        public async Task OdpowiedzNaPytanie(int quizId, int pytanieId, int odpowiedzId, long czasOdpowiedzi, string uzytkownikId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                // Aktualizujemy wyniki w stanie quizu
                quizState.ZapiszOdpowiedz(uzytkownikId, pytanieId, odpowiedzId, czasOdpowiedzi);

                // Wysyłamy informację o odpowiedzi do wszystkich w grupie (opcjonalnie)
                await Clients.Group(quizIdStr).SendAsync("OdpowiedzNaPytanie", uzytkownikId, pytanieId, odpowiedzId, czasOdpowiedzi);

                // Wysyłamy aktualne wyniki do wszystkich (lub tylko do zainteresowanych)
                await Clients.Group(quizIdStr).SendAsync("WynikiCzęściowe", quizState.Wyniki);

                // Jeśli wszyscy odpowiedzieli, można przejść do następnego pytania (logika w kontrolerze lub tutaj)
                if (quizState.CzyWszyscyOdpowiedzieli(pytanieId))
                {
                    await Clients.Group(quizIdStr).SendAsync("PytanieZakonczone");
                }
            }
        }

        public async Task ZakonczQuiz(int quizId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryRemove(quizIdStr, out var quizState)) // Usuwamy stan quizu po zakończeniu
            {
                await Clients.Group(quizIdStr).SendAsync("KoniecQuizu", quizState.Wyniki);
            }
        }

        public async Task<List<RankingUzytkownika>> PobierzRanking(int quizId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
               if (quizState.Uzytkownicy.Count < 2) 
        {
            quizState.Uzytkownicy.TryAdd("testuser1", new Uzytkownik { Nick = "Testowy1", Id = "testuser1", Punkty = 0 });
            quizState.Uzytkownicy.TryAdd("testuser2", new Uzytkownik { Nick = "Testowy2", Id = "testuser2", Punkty = 0 });
        }
        return quizState.PobierzRanking();
            }
            return new List<RankingUzytkownika>();
        }
    }

    // Klasa przechowująca stan quizu
    public class QuizState
    {
        public bool CzyRozpoczelo { get; set; } = false;
        public object AktualnePytanie { get; set; } // Serializowany obiekt pytania
        public ConcurrentDictionary<string, Uzytkownik> Uzytkownicy { get; set; } = new();
        public ConcurrentDictionary<string, Dictionary<int, WynikUzytkownika>> Wyniki { get; set; } = new(); // UzytkownikId -> (PytanieId -> WynikUzytkownika)

        public void DodajUzytkownika(string uzytkownikId, string uzytkownikNick, string connectionId)
        {
            if (string.IsNullOrEmpty(uzytkownikNick))
            {
                uzytkownikNick = "guest"; // Przypisujemy "guest"
            }

            Uzytkownik nowyUzytkownik = new Uzytkownik { Nick = uzytkownikNick, ConnectionId = connectionId };
            Uzytkownicy.TryAdd(uzytkownikId, nowyUzytkownik); // Używamy TryAdd

            // Alternatywnie (jeśli chcesz nadpisać istniejącego użytkownika):
            // Uzytkownicy[uzytkownikId] = nowyUzytkownik;

            Wyniki.TryAdd(uzytkownikId, new Dictionary<int, WynikUzytkownika>());
        }

        public void ZapiszOdpowiedz(string uzytkownikId, int pytanieId, int odpowiedzId, long czasOdpowiedzi)
        {
            if (Wyniki.TryGetValue(uzytkownikId, out var wynikiUzytkownika))
            {
                wynikiUzytkownika[pytanieId] = new WynikUzytkownika { OdpowiedzId = odpowiedzId, CzasOdpowiedzi = czasOdpowiedzi };
            }
        }


        public bool CzyWszyscyOdpowiedzieli(int pytanieId)
        {
            foreach (var uzytkownik in Uzytkownicy.Keys)
            {
                if (!Wyniki.ContainsKey(uzytkownik) || !Wyniki[uzytkownik].ContainsKey(pytanieId))
                {
                    return false; // Przynajmniej jeden użytkownik nie odpowiedział
                }
            }
            return true; // Wszyscy odpowiedzieli
        }

        public List<RankingUzytkownika> PobierzRanking()
        {

            return Uzytkownicy.Select(u => new RankingUzytkownika
            {
                Nick = u.Value.Nick,
                Punkty = ObliczPunkty(u.Key) // Dodaj metodę ObliczPunkty (poniżej)
            })
            .OrderByDescending(r => r.Punkty)
            .ToList();
        }

        private int ObliczPunkty(string uzytkownikId)
        {
            int punkty = 0;
            if (Wyniki.ContainsKey(uzytkownikId))
            {
                foreach (var wynik in Wyniki[uzytkownikId].Values)
                {
                    // Dodaj logikę obliczania punktów na podstawie odpowiedzi i czasu
                    // To zależy od twojego algorytmu punktowania
                    punkty += (int)(1000 * (1 - (double)wynik.CzasOdpowiedzi / 30000)); // Przykład
                }
            }
            return punkty;
        }
    }

    public class Uzytkownik
    {
        public string Id { get; set; }
        public string Nick { get; set; }
        public int Punkty { get; set; }
        public string ConnectionId { get; set; } // Dodajemy ConnectionId
    }

    public class WynikUzytkownika
    {
        public int OdpowiedzId { get; set; }
        public long CzasOdpowiedzi { get; set; }
    }
}