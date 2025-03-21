﻿using Microsoft.AspNetCore.SignalR;
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

            var httpContext = Context.GetHttpContext();
            var dbContext = httpContext.RequestServices.GetService<QuizAppContext>();
            var pytania = await dbContext.Pytanie
                .Include(p => p.Odpowiedzi)
                .Where(p => p.QuizId == quizId)
                .ToListAsync();

            // Zainicjalizuj stan quizu
            quizState.InicjalizujPytania(pytania);

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
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                try
                {
                    var context = Context.GetHttpContext();
                    var dbContext = context.RequestServices.GetService<QuizAppContext>();

                    // Pobranie poprawnej odpowiedzi
                    var pytanie = await dbContext.Pytanie
                        .Include(p => p.Odpowiedzi)
                        .FirstOrDefaultAsync(p => p.Id == pytanieId);

                    if (pytanie == null)
                    {
                        _logger.LogError($"Nie znaleziono pytania o ID {pytanieId}.");
                        await Clients.Caller.SendAsync("BladOdpowiedzi", "Nie znaleziono pytania.");
                        return;
                    }

                    var poprawnaOdpowiedz = pytanie.Odpowiedzi.Any(o => o.Id == odpowiedzId && o.CzyPoprawna);
                    int punktyZaOdpowiedz = poprawnaOdpowiedz ? (int)(1000 * (1 - (double)czasOdpowiedzi / 30000)) : 0;

                    if (quizState.Uzytkownicy.TryGetValue(uzytkownikId, out var user))
                    {
                        // Zapobieganie podwójnemu przetwarzaniu odpowiedzi
                        if (user.Odpowiedzi.ContainsKey(pytanieId))
                        {
                            _logger.LogWarning($"Odpowiedź na pytanie {pytanieId} została już przetworzona dla użytkownika {uzytkownikId}.");
                            return;
                        }

                        user.Punkty += punktyZaOdpowiedz;
                        user.Odpowiedzi[pytanieId] = odpowiedzId;
                        user.CzasyOdpowiedzi[pytanieId] = czasOdpowiedzi;
                    }

                    // Pobranie nowego rankingu
                    var ranking = quizState.PobierzRanking();

                    _logger.LogInformation($"Ranking po odpowiedzi: {JsonSerializer.Serialize(ranking)}");

                    // Wysłanie rankingu do wszystkich
                    await Clients.Group(quizIdStr).SendAsync("AktualizujRanking", ranking);

                    // Przejście do kolejnego pytania
                    if (quizState.AktualnePytanieIndex < quizState.Pytania?.Count - 1)
                    {
                        quizState.AktualnePytanieIndex++;
                        var nowePytanie = quizState.Pytania[quizState.AktualnePytanieIndex];
                        await Clients.Group(quizIdStr).SendAsync("PokazPytanie", nowePytanie);
                    }
                    else
                    {
                        // Jeśli quiz się skończył -> pokazujemy końcowy ranking
                        await Clients.Group(quizIdStr).SendAsync("KoniecQuizu", ranking);
                        await Clients.Caller.SendAsync("KoniecQuizuKlient", ranking); // Dodane przekierowanie do Wynik.cshtml
                    }
                    //Potwierdzenie odpowiedzi
                    await Clients.Caller.SendAsync("PotwierdzenieOdpowiedzi", ranking, quizState.AktualnePytanieIndex >= quizState.Pytania?.Count - 1);
                    _logger.LogInformation($"Użytkownik {uzytkownikId} odpowiedział na pytanie {pytanieId}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas przetwarzania odpowiedzi.");
                    await Clients.Caller.SendAsync("BladOdpowiedzi", "Wystąpił błąd podczas przetwarzania odpowiedzi.");
                }
            }
        }


        private async Task PokazNastepnePytanie(int quizId)
        {
            string quizIdStr = quizId.ToString();
            if (_quizStates.TryGetValue(quizIdStr, out var quizState))
            {
                var pytanie = quizState.Pytania[quizState.AktualnePytanieIndex];
                await Clients.Group(quizIdStr).SendAsync("PokazPytanie", pytanie);
                StartTimer(quizIdStr);
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

        public void InicjalizujPytania(List<Pytanie> pytania)
        {
            Pytania = pytania;
            AktualnePytanieIndex = 0;
        }

        public void DodajUzytkownika(string uzytkownikId, string uzytkownikNick, string connectionId)
        {
            Uzytkownik nowyUzytkownik = new Uzytkownik { Nick = uzytkownikNick, Id = uzytkownikId, ConnectionId = connectionId };
            Uzytkownicy.TryAdd(uzytkownikId, nowyUzytkownik);
            Wyniki.TryAdd(uzytkownikId, new Dictionary<int, WynikUzytkownika>());
        }

        public List<RankingUzytkownika> PobierzRanking()
        {
            return Uzytkownicy.Select(u => new RankingUzytkownika
            {
                Nick = u.Value.Nick,
                Punkty = ObliczPunkty(u.Value)
            }).OrderByDescending(r => r.Punkty).ToList();
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



    }

    public class Uzytkownik
    {
        public string Id { get; set; }
        public string Nick { get; set; }
        public int Punkty { get; set; }
        public string ConnectionId { get; set; }
        public Dictionary<int, int> Odpowiedzi { get; set; } = new Dictionary<int, int>(); // Dodajemy słownik na odpowiedzi
        public Dictionary<int, long> CzasyOdpowiedzi { get; set; } = new Dictionary<int, long>(); // Dodajemy słownik na czasy odpowiedzi
    }

    public class WynikUzytkownika
    {
        public int OdpowiedzId { get; set; }
        public long CzasOdpowiedzi { get; set; }
    }
}