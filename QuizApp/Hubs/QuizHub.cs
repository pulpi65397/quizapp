using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using QuizApp.Models;
using QuizApp.Controllers;
using QuizApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace QuizApp.Hubs
{
    public class QuizHub : Hub
    {
        // Słownik przechowujący stan quizu dla każdego pokoju (quizId)
        private static readonly ConcurrentDictionary<string, QuizState> _quizStates = new();
        private readonly ILogger<QuizState> _logger;

        public QuizHub(ILogger<QuizState> logger)
        {
            _logger = logger;
        }


        public async Task DołączDoQuizu(int quizId, string uzytkownikId, string uzytkownikNick)
        {
            string quizIdStr = quizId.ToString();

            // Dodajemy użytkownika do grupy SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, quizIdStr);

            // Pobieramy lub tworzymy stan quizu
            //var quizState = _quizStates.GetOrAdd(quizIdStr, _ => new QuizState());
            var quizState = _quizStates.GetOrAdd(quizIdStr, _ => new QuizState(_logger));

            if (!quizState.CzyRozpoczelo)
            {
                var httpContext = Context.GetHttpContext();
                var dbContext = httpContext.RequestServices.GetService<QuizAppContext>();
                var pytania = await dbContext.Pytanie
                    .Include(p => p.Odpowiedzi)
                    .Where(p => p.QuizId == quizId)
                    .ToListAsync();
                quizState.InicjalizujPytania(pytania);
            }

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
                // Pobierz pytania z bazy danych
                var httpContext = Context.GetHttpContext();
                var dbContext = httpContext.RequestServices.GetService<QuizAppContext>();
                var pytania = await dbContext.Pytanie
                    .Include(p => p.Odpowiedzi)
                    .Where(p => p.QuizId == quizId)
                    .ToListAsync();

                // Zainicjalizuj stan quizu
                quizState.InicjalizujPytania(pytania);
                quizState.CzyRozpoczelo = true;

                await Clients.Group(quizIdStr).SendAsync("RozpocznijQuiz");
                await PokazNastepnePytanie(quizId);
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
            if (!_quizStates.TryGetValue(quizIdStr, out var quizState)) return;

            try
            {
                var httpContext = Context.GetHttpContext();
                var dbContext = httpContext.RequestServices.GetService<QuizAppContext>();

                // 1. Walidacja pytania
                var pytanie = await dbContext.Pytanie
                    .Include(p => p.Odpowiedzi)
                    .FirstOrDefaultAsync(p => p.Id == pytanieId);

                if (pytanie == null)
                {
                    await Clients.Caller.SendAsync("BladOdpowiedzi", "Nieprawidłowe pytanie");
                    return;
                }

                // 2. Obliczanie punktów
                var poprawnaOdpowiedz = pytanie.Odpowiedzi.Any(o => o.Id == odpowiedzId && o.CzyPoprawna);
                var punkty = poprawnaOdpowiedz ? (int)(1000 * (1 - (double)czasOdpowiedzi / 30000)) : 0;

                // 3. Synchronizacja wątków
                lock (quizState.SyncObject)
                {
                    if (!quizState.Uzytkownicy.TryGetValue(uzytkownikId, out var uzytkownik))
                    {
                        _logger.LogWarning($"Użytkownik {uzytkownikId} nie istnieje w stanie quizu");
                        return;
                    }

                    if (uzytkownik.Odpowiedzi.ContainsKey(pytanieId)) return;

                    // 4. Aktualizacja stanu użytkownika
                    uzytkownik.Punkty += punkty;
                    uzytkownik.Odpowiedzi[pytanieId] = odpowiedzId;
                    uzytkownik.CzasyOdpowiedzi[pytanieId] = czasOdpowiedzi;
                }

                // 5. Sprawdź czy wszyscy odpowiedzieli
                var wszyscyOdpowiedzieli = quizState.CzyWszyscyOdpowiedzieli(pytanieId);

                if (wszyscyOdpowiedzieli)
                {
                    quizState.AktualnePytanieIndex++;

                    // 6. Obsługa końca quizu
                    if (quizState.AktualnePytanieIndex >= quizState.Pytania.Count)
                    {
                        await ZakonczQuiz(quizId);
                    }
                    else
                    {
                        // 7. Wyślij następne pytanie
                        await Clients.Group(quizIdStr).SendAsync("PokazPytanie",
                            quizState.Pytania[quizState.AktualnePytanieIndex]);
                    }
                }

                // 8. Aktualizuj ranking
                var ranking = quizState.PobierzRanking();
                await Clients.Group(quizIdStr).SendAsync("AktualizujRanking", ranking);

                // 9. Potwierdź odpowiedź
                await Clients.Caller.SendAsync("PotwierdzenieOdpowiedzi",
                    ranking,
                    quizState.AktualnePytanieIndex >= quizState.Pytania.Count - 1);

                _logger.LogInformation($"Odpowiedź użytkownika {uzytkownikId} na pytanie {pytanieId} przetworzona");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Błąd podczas przetwarzania odpowiedzi: Quiz {quizId}, Pytanie {pytanieId}");
                await Clients.Caller.SendAsync("BladOdpowiedzi", "Wystąpił błąd systemowego przetwarzania odpowiedzi");
            }
        }

        private void StartIndywidualnyTimer(string quizIdStr, string uzytkownikId)
        {
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                // Usuń istniejący timer
                if (quizState.TimeryUzytkownikow.TryGetValue(uzytkownikId, out var staryTimer))
                {
                    staryTimer?.Dispose();
                }

                var timer = new Timer(async _ =>
                {
                    try
                    {
                        if (!_quizStates.TryGetValue(quizIdStr, out var qs) ||
                            !qs.Uzytkownicy.TryGetValue(uzytkownikId, out var user)) return;

                        // Pobierz AKTUALNE pytanie z GLOBALNEGO indeksu
                        var aktualnePytanie = qs.Pytania?.ElementAtOrDefault(qs.AktualnePytanieIndex);
                        if (aktualnePytanie == null) return;

                        // Sprawdź, czy użytkownik NIE odpowiedział na bieżące pytanie
                        if (!user.Odpowiedzi.ContainsKey(aktualnePytanie.Id))
                        {
                            // Wymuś odpowiedź (np. -1 oznacza brak odpowiedzi)
                            await OdpowiedzNaPytanie(
                                int.Parse(quizIdStr),
                                aktualnePytanie.Id,
                                -1,
                                30000,
                                uzytkownikId
                            );
                        }

                        // NIE zwiększaj tutaj indeksu! To zadanie metody OdpowiedzNaPytanie
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Błąd w timerze indywidualnym");
                    }
                    await SprawdzCzyKoniec(quizIdStr);
                }, null, 30000, Timeout.Infinite);

                quizState.TimeryUzytkownikow[uzytkownikId] = timer;
            }
        }


        public async Task PokazNastepnePytanie(int quizId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                if (quizState.AktualnePytanieIndex >= quizState.Pytania.Count)
                {
                    await ZakonczQuiz(quizId);
                    return;
                }

                var pytanie = quizState.Pytania[quizState.AktualnePytanieIndex];
                await Clients.Group(quizIdStr).SendAsync("PokazPytanie", pytanie);
            }
        }

        private async Task SprawdzCzyKoniec(string quizIdStr)
        {
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                if (quizState.AktualnePytanieIndex >= quizState.Pytania.Count)
                {
                    await ZakonczQuiz(int.Parse(quizIdStr));
                }
            }
        }

        public async Task ZakonczQuiz(int quizId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryRemove(quizIdStr, out var quizState))
            {
                // Wyślij specjalne zdarzenie kończące
                await Clients.Group(quizIdStr).SendAsync("KoniecQuizuKlient", quizState.PobierzRanking());

                // Dodatkowo: wyczyść wszystkie timery
                foreach (var timer in quizState.TimeryUzytkownikow.Values)
                {
                    timer?.Dispose();
                }
            }
        }

        public async Task<List<RankingUzytkownika>> PobierzRanking(int quizId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                return quizState.PobierzRanking();
            }
            return new List<RankingUzytkownika>();
        }

        private void StartTimer(string quizIdStr)
        {
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                quizState.Timer = new Timer(async _ =>
                {
                    await PokazNastepnePytanie(int.Parse(quizIdStr));
                }, null, 30000, Timeout.Infinite); // 30 sekund, jednorazowo
            }
        }
    }

    // Klasa przechowująca stan quizu
    public class QuizState
    {
        public ConcurrentDictionary<string, int> AktualnePytanieIndexUzytkownikow { get; } = new();
        public readonly object SyncObject = new object();

        private readonly ILogger<QuizState> _logger;
        public QuizState(ILogger<QuizState> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public Timer Timer { get; set; }
        public bool CzyRozpoczelo { get; set; } = false;
        public object AktualnePytanie { get; set; }
        public int AktualnePytanieIndex { get; set; }
        public List<Pytanie> Pytania { get; set; }
        public ConcurrentDictionary<string, Uzytkownik> Uzytkownicy { get; set; } = new ConcurrentDictionary<string, Uzytkownik>(); // Dodajemy Uzytkownicy
        public ConcurrentDictionary<string, Dictionary<int, WynikUzytkownika>> Wyniki { get; set; } = new ConcurrentDictionary<string, Dictionary<int, WynikUzytkownika>>(); // Dodajemy Wyniki
        public ConcurrentDictionary<string, Timer> TimeryUzytkownikow { get; } = new();

        public void InicjalizujPytania(List<Pytanie> pytania)
        {
            if (Pytania == null || !Pytania.Any())
            {
                Pytania = pytania;
                AktualnePytanieIndex = 0;
            }
        }

        public void DodajUzytkownika(string uzytkownikId, string uzytkownikNick, string connectionId)
        {
            AktualnePytanieIndexUzytkownikow.TryAdd(uzytkownikId, 0);
            Uzytkownik nowyUzytkownik = new Uzytkownik { Nick = uzytkownikNick, Id = uzytkownikId, ConnectionId = connectionId };
            Uzytkownicy.TryAdd(uzytkownikId, nowyUzytkownik);
            Wyniki.TryAdd(uzytkownikId, new Dictionary<int, WynikUzytkownika>());
        }
        public void AktualizujPostep(string uzytkownikId)
        {
            if (AktualnePytanieIndexUzytkownikow.TryGetValue(uzytkownikId, out var index))
            {
                AktualnePytanieIndexUzytkownikow[uzytkownikId] = index + 1;
            }
        }

        public List<RankingUzytkownika> PobierzRanking()
        {
            var ranking = Uzytkownicy.Select(u => new RankingUzytkownika
            {
                Nick = u.Value.Nick,
                Punkty = ObliczPunkty(u.Value)
            }).OrderByDescending(r => r.Punkty).ToList();

            _logger.LogInformation("[DEBUG] Wygenerowany ranking: " + JsonSerializer.Serialize(ranking));
            return ranking;
        }

        public bool CzyWszyscyOdpowiedzieli(int pytanieId)
        {
            foreach (var uzytkownik in Uzytkownicy.Values)
            {
                if (!uzytkownik.Odpowiedzi.ContainsKey(pytanieId))
                {
                    return false; // Przynajmniej jeden użytkownik nie odpowiedział
                }
            }
            return true; // Wszyscy odpowiedzieli
        }

        public int ObliczPunkty(Uzytkownik user)
        {
            if (Pytania == null) return 0;

            return user.Odpowiedzi.Sum(o =>
            {
                if (!CzyPoprawnaOdpowiedz(o.Key, o.Value)) return 0;

                var czas = user.CzasyOdpowiedzi.TryGetValue(o.Key, out var t) ? t : 30000;
                return Math.Max((int)(1000 * (1 - (double)czas / 30000)), 0);
            });
        }

        public bool CzyPoprawnaOdpowiedz(int pytanieId, int odpowiedzId)
        {
            var pytanie = Pytania?.FirstOrDefault(p => p.Id == pytanieId);

            if (pytanie == null)
            {
                _logger.LogError($"Nie znaleziono pytania o ID {pytanieId}");
                return false;
            }

            var odpowiedz = pytanie.Odpowiedzi?.FirstOrDefault(o => o.Id == odpowiedzId);
            if (odpowiedz == null)
            {
                _logger.LogError($"Nie znaleziono odpowiedzi o ID {odpowiedzId} dla pytania {pytanieId}");
                return false;
            }

            _logger.LogInformation($"Sprawdzanie odpowiedzi. Pytanie ID: {pytanieId}, Odpowiedz ID: {odpowiedzId}, Poprawna: {odpowiedz.CzyPoprawna}");

            return odpowiedz.CzyPoprawna;
        }

        public void PrzejdzDoNastepnegoPytania()
        {
            if (AktualnePytanieIndex < Pytania.Count - 1)
            {
                AktualnePytanieIndex++;
            }
        }



    }

    public class Uzytkownik
    {
        public string Id { get; set; }
        public string Nick { get; set; }
        public int Punkty { get; set; }
        public string ConnectionId { get; set; }

        public int AktualnePytanieIndex { get; set; } = 0;
        public Dictionary<int, int> Odpowiedzi { get; set; } = new Dictionary<int, int>(); // Dodajemy słownik na odpowiedzi
        public Dictionary<int, long> CzasyOdpowiedzi { get; set; } = new Dictionary<int, long>(); // Dodajemy słownik na czasy odpowiedzi
    }

    public class WynikUzytkownika
    {
        public int OdpowiedzId { get; set; }
        public long CzasOdpowiedzi { get; set; }
    }
}