using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuizApp.Data;
using QuizApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Models;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new QuizAppContext(
            serviceProvider.GetRequiredService<DbContextOptions<QuizAppContext>>()))
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Dodaj administratora (nawet jeśli inne dane istnieją)
            await SeedAdminUser(userManager);

            // 2. Dodaj resztę danych TYLKO jeśli baza jest pusta
            if (!context.Quiz.Any())
            {
                // Dodaj quizy
                var quiz1 = new Quiz { Tytul = "Quiz wiedzy o punk rocku", LiczbaPytan = 5, Dziedzina = "Muzyka" };
                var quiz2 = new Quiz { Tytul = "Podstawy technologii WWW", LiczbaPytan = 5, Dziedzina = "Informatyka" };
                var quiz3 = new Quiz { Tytul = "Quiz wiedzy ogólnej", LiczbaPytan = 5, Dziedzina = "Geografia" };
                var quiz4 = new Quiz { Tytul = "Quiz wiedzy o telefonach", LiczbaPytan = 5, Dziedzina = "Telekomunikacja" };

                context.Quiz.AddRange(quiz1, quiz2, quiz3, quiz4);
                await context.SaveChangesAsync();

                // Dodaj pytania
                var pytania = new[]
                {
                    new Pytanie { Tekst = "W którym roku powstał zespół Farben Lehre?", QuizId = quiz1.Id },
                    new Pytanie { Tekst = "Który album Farben Lehre zawiera utwór \"Helikoptery\"?", QuizId = quiz1.Id },
                    new Pytanie { Tekst = "Który zespół punk rockowy jest znany z piosenki \"Defekt Mózgu\"?", QuizId = quiz1.Id },
                    new Pytanie { Tekst = "W którym roku zespół Sedes wydał swój debiutancki album \"Wszyscy pokutujemy\"?", QuizId = quiz1.Id },
                    new Pytanie { Tekst = "Który zespół punk rockowy wydał album \"Zdrada\"?", QuizId = quiz1.Id },

                    new Pytanie { Tekst = "Kto wymyślił WWW?", QuizId = quiz2.Id },
                    new Pytanie { Tekst = "W którym roku wynaleziono World Wide Web(WWW)?", QuizId = quiz2.Id },
                    new Pytanie { Tekst = "Który z poniższych języków jest językiem programowania po stronie klienta?", QuizId = quiz2.Id },
                    new Pytanie { Tekst = "Który z poniższych języków jest językiem programowania po stronie serwera?", QuizId = quiz2.Id },
                    new Pytanie { Tekst = "Który z poniższych języków jest językiem programowania?", QuizId = quiz2.Id },

                    new Pytanie { Tekst = "Jaki jest największy ocean na Ziemi?", QuizId = quiz3.Id },
                    new Pytanie { Tekst = "Kto napisał \"Hamleta\"?", QuizId = quiz3.Id },
                    new Pytanie { Tekst = "W którym roku rozpoczęła się II wojna światowa?", QuizId = quiz3.Id },
                    new Pytanie { Tekst = "Jaki jest najwyższy szczyt świata?", QuizId = quiz3.Id },
                    new Pytanie { Tekst = "Który z poniższych języków jest językiem programowania?", QuizId = quiz3.Id },

                    new Pytanie { Tekst = "U jakiego operatora był najchętniej kupowany iPhone?", QuizId = quiz4.Id },
                    new Pytanie { Tekst = "W którym roku wydano pierwszy smartfon z systemem Android?", QuizId = quiz4.Id },
                    new Pytanie { Tekst = "Który producent telefonów jako pierwszy wprowadził ekran dotykowy w swoim urządzeniu?", QuizId = quiz4.Id },
                    new Pytanie { Tekst = "Który model telefonu miał pierwszy aparat fotograficzny o rozdzielczości 108 MP?", QuizId = quiz4.Id },
                    new Pytanie { Tekst = "Która firma wprowadziła na rynek pierwszy składany telefon z elastycznym ekranem?", QuizId = quiz4.Id },


                };

                context.Pytanie.AddRange(pytania);
                await context.SaveChangesAsync();

                // Dodaj odpowiedzi
                context.Odpowiedz.AddRange(
                    new Odpowiedz { Tekst = "1983", CzyPoprawna = false, PytanieId = pytania[0].Id },
                    new Odpowiedz { Tekst = "1986", CzyPoprawna = true, PytanieId = pytania[0].Id },
                    new Odpowiedz { Tekst = "1989", CzyPoprawna = false, PytanieId = pytania[0].Id },
                    new Odpowiedz { Tekst = "1992", CzyPoprawna = false, PytanieId = pytania[0].Id },

                    new Odpowiedz { Tekst = "\"My maszyny\"", CzyPoprawna = false, PytanieId = pytania[1].Id },
                    new Odpowiedz { Tekst = "\"Pozytywka\"", CzyPoprawna = false, PytanieId = pytania[1].Id },
                    new Odpowiedz { Tekst = "\"Atomowe zabawki\"", CzyPoprawna = true, PytanieId = pytania[1].Id },
                    new Odpowiedz { Tekst = "\"Garażówka\"", CzyPoprawna = false, PytanieId = pytania[1].Id },

                    new Odpowiedz { Tekst = "Farben Lehre", CzyPoprawna = false, PytanieId = pytania[2].Id },
                    new Odpowiedz { Tekst = "Defekt Mózgu", CzyPoprawna = true, PytanieId = pytania[2].Id },
                    new Odpowiedz { Tekst = "Sedes", CzyPoprawna = false, PytanieId = pytania[2].Id },
                    new Odpowiedz { Tekst = "KSU", CzyPoprawna = false, PytanieId = pytania[2].Id },

                    new Odpowiedz { Tekst = "1985", CzyPoprawna = false, PytanieId = pytania[3].Id },
                    new Odpowiedz { Tekst = "1987", CzyPoprawna = false, PytanieId = pytania[3].Id },
                    new Odpowiedz { Tekst = "1990", CzyPoprawna = false, PytanieId = pytania[3].Id },
                    new Odpowiedz { Tekst = "1992", CzyPoprawna = true, PytanieId = pytania[3].Id },

                    new Odpowiedz { Tekst = "Farben Lehre", CzyPoprawna = true, PytanieId = pytania[4].Id },
                    new Odpowiedz { Tekst = "Defekt Mózgu", CzyPoprawna = false, PytanieId = pytania[4].Id },
                    new Odpowiedz { Tekst = "Sedes", CzyPoprawna = false, PytanieId = pytania[4].Id },
                    new Odpowiedz { Tekst = "Armia", CzyPoprawna = false, PytanieId = pytania[4].Id },

                    new Odpowiedz { Tekst = "Elon Musk", CzyPoprawna = false, PytanieId = pytania[5].Id },
                    new Odpowiedz { Tekst = "Bill Gates", CzyPoprawna = false, PytanieId = pytania[5].Id },
                    new Odpowiedz { Tekst = "Sir Timothy Berners-Lee", CzyPoprawna = true, PytanieId = pytania[5].Id },
                    new Odpowiedz { Tekst = "Mark Zuckerberg", CzyPoprawna = false, PytanieId = pytania[5].Id },

                    new Odpowiedz { Tekst = "1989", CzyPoprawna = true, PytanieId = pytania[6].Id },
                    new Odpowiedz { Tekst = "1990", CzyPoprawna = false, PytanieId = pytania[6].Id },
                    new Odpowiedz { Tekst = "1991", CzyPoprawna = false, PytanieId = pytania[6].Id },
                    new Odpowiedz { Tekst = "1992", CzyPoprawna = false, PytanieId = pytania[6].Id },

                    new Odpowiedz { Tekst = "HTML", CzyPoprawna = false, PytanieId = pytania[7].Id },
                    new Odpowiedz { Tekst = "CSS", CzyPoprawna = false, PytanieId = pytania[7].Id },
                    new Odpowiedz { Tekst = "JavaScript", CzyPoprawna = true, PytanieId = pytania[7].Id },
                    new Odpowiedz { Tekst = "PHP", CzyPoprawna = false, PytanieId = pytania[7].Id },

                    new Odpowiedz { Tekst = "HTML", CzyPoprawna = false, PytanieId = pytania[8].Id },
                    new Odpowiedz { Tekst = "CSS", CzyPoprawna = false, PytanieId = pytania[8].Id },
                    new Odpowiedz { Tekst = "JavaScript", CzyPoprawna = false, PytanieId = pytania[8].Id },
                    new Odpowiedz { Tekst = "PHP", CzyPoprawna = true, PytanieId = pytania[8].Id },

                    new Odpowiedz { Tekst = "HTML", CzyPoprawna = false, PytanieId = pytania[9].Id },
                    new Odpowiedz { Tekst = "CSS", CzyPoprawna = false, PytanieId = pytania[9].Id },
                    new Odpowiedz { Tekst = "SQL", CzyPoprawna = false, PytanieId = pytania[9].Id },
                    new Odpowiedz { Tekst = "PHP", CzyPoprawna = true, PytanieId = pytania[9].Id },

                    new Odpowiedz { Tekst = "Ocean Atlantycki", CzyPoprawna = false, PytanieId = pytania[10].Id },
                    new Odpowiedz { Tekst = "Ocean Arktyczny", CzyPoprawna = false, PytanieId = pytania[10].Id },
                    new Odpowiedz { Tekst = "Ocean Indyjski", CzyPoprawna = false, PytanieId = pytania[10].Id },
                    new Odpowiedz { Tekst = "Ocean Spokojny", CzyPoprawna = true, PytanieId = pytania[10].Id },

                    new Odpowiedz { Tekst = "Karol Dickens", CzyPoprawna = false, PytanieId = pytania[11].Id },
                    new Odpowiedz { Tekst = "Jane Austen", CzyPoprawna = false, PytanieId = pytania[11].Id },
                    new Odpowiedz { Tekst = "William Shakespeare", CzyPoprawna = true, PytanieId = pytania[11].Id },
                    new Odpowiedz { Tekst = "Fiodor Dostojewski", CzyPoprawna = false, PytanieId = pytania[11].Id },

                    new Odpowiedz { Tekst = "206", CzyPoprawna = true, PytanieId = pytania[12].Id },
                    new Odpowiedz { Tekst = "213", CzyPoprawna = false, PytanieId = pytania[12].Id },
                    new Odpowiedz { Tekst = "220", CzyPoprawna = false, PytanieId = pytania[12].Id },
                    new Odpowiedz { Tekst = "230", CzyPoprawna = false, PytanieId = pytania[12].Id },

                    new Odpowiedz { Tekst = "1914", CzyPoprawna = false, PytanieId = pytania[13].Id },
                    new Odpowiedz { Tekst = "1939", CzyPoprawna = true, PytanieId = pytania[13].Id },
                    new Odpowiedz { Tekst = "1945", CzyPoprawna = false, PytanieId = pytania[13].Id },
                    new Odpowiedz { Tekst = "1929", CzyPoprawna = false, PytanieId = pytania[13].Id },

                    new Odpowiedz { Tekst = "Giewont", CzyPoprawna = false, PytanieId = pytania[14].Id },
                    new Odpowiedz { Tekst = "Mount Everest", CzyPoprawna = true, PytanieId = pytania[14].Id },
                    new Odpowiedz { Tekst = "Kangchendzonga", CzyPoprawna = false, PytanieId = pytania[14].Id },
                    new Odpowiedz { Tekst = "Lhotse", CzyPoprawna = false, PytanieId = pytania[14].Id },

                    new Odpowiedz { Tekst = "T-Mobile", CzyPoprawna = false, PytanieId = pytania[15].Id },
                    new Odpowiedz { Tekst = "Play", CzyPoprawna = false, PytanieId = pytania[15].Id },
                    new Odpowiedz { Tekst = "Plus", CzyPoprawna = false, PytanieId = pytania[15].Id },
                    new Odpowiedz { Tekst = "Orange", CzyPoprawna = true, PytanieId = pytania[15].Id },

                    new Odpowiedz { Tekst = "2005", CzyPoprawna = false, PytanieId = pytania[16].Id },
                    new Odpowiedz { Tekst = "2007", CzyPoprawna = false, PytanieId = pytania[16].Id },
                    new Odpowiedz { Tekst = "2008", CzyPoprawna = true, PytanieId = pytania[16].Id },
                    new Odpowiedz { Tekst = "2010", CzyPoprawna = false, PytanieId = pytania[16].Id },

                    new Odpowiedz { Tekst = "Nokia", CzyPoprawna = false, PytanieId = pytania[17].Id },
                    new Odpowiedz { Tekst = "Apple", CzyPoprawna = false, PytanieId = pytania[17].Id },
                    new Odpowiedz { Tekst = "LG", CzyPoprawna = false, PytanieId = pytania[17].Id },
                    new Odpowiedz { Tekst = "IBM", CzyPoprawna = true, PytanieId = pytania[17].Id },

                    new Odpowiedz { Tekst = "Samsung Galaxy S20 Ultra", CzyPoprawna = false, PytanieId = pytania[18].Id },
                    new Odpowiedz { Tekst = "Xiaomi Mi Note 10", CzyPoprawna = true, PytanieId = pytania[18].Id },
                    new Odpowiedz { Tekst = "Sony Xperia 1 II", CzyPoprawna = false, PytanieId = pytania[18].Id },
                    new Odpowiedz { Tekst = "Huawei P40 Pro", CzyPoprawna = false, PytanieId = pytania[18].Id },

                    new Odpowiedz { Tekst = "Motorola", CzyPoprawna = false, PytanieId = pytania[19].Id },
                    new Odpowiedz { Tekst = "Samsung", CzyPoprawna = true, PytanieId = pytania[19].Id },
                    new Odpowiedz { Tekst = "Huawei", CzyPoprawna = false, PytanieId = pytania[19].Id },
                    new Odpowiedz { Tekst = "LG", CzyPoprawna = false, PytanieId = pytania[19].Id }




                );

                // Dodaj zwykłych użytkowników
                var uzytkownik1 = new Uzytkownik { Nick = "Piotr" };
                var uzytkownik2 = new Uzytkownik { Nick = "Marek" };
                var uzytkownik3 = new Uzytkownik { Nick = "Piotr" };
                var uzytkownik4 = new Uzytkownik { Nick = "Piotr" };
                context.Uzytkownik.AddRange(uzytkownik1, uzytkownik2, uzytkownik3, uzytkownik4);
                await context.SaveChangesAsync();

                // Dodaj wyniki
                context.Wynik.AddRange(
                    new Wynik
                    {
                        QuizId = quiz1.Id,
                        UzytkownikId = uzytkownik1.Id,
                        Punkty = 4731,
                        CzasyOdpowiedziJson = "[]",
                        OdpowiedziJson = "[]"
                    },
                    new Wynik
                    {
                        QuizId = quiz1.Id,
                        UzytkownikId = uzytkownik2.Id,
                        Punkty = 4830,
                        CzasyOdpowiedziJson = "[]",
                        OdpowiedziJson = "[]"
                    },
                    new Wynik
                    {
                         QuizId = quiz1.Id,
                         UzytkownikId = uzytkownik3.Id,
                         Punkty = 4598,
                         CzasyOdpowiedziJson = "[]",
                         OdpowiedziJson = "[]"
                    },
                    new Wynik
                    {
                        QuizId = quiz1.Id,
                        UzytkownikId = uzytkownik4.Id,
                        Punkty = 3331,
                        CzasyOdpowiedziJson = "[]",
                        OdpowiedziJson = "[]"
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
    {
        var adminUser = new ApplicationUser
        {
            UserName = "administrator" // Wartość "administrator" (małe litery)
        };

        if (await userManager.FindByNameAsync(adminUser.UserName) == null)
        {
            // Utwórz użytkownika z hasłem
            var result = await userManager.CreateAsync(adminUser, "Admin1!");

            if (!result.Succeeded)
            {
                // Zaloguj błędy (np. do konsoli)
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Błąd: {error.Description}");
                }
                throw new Exception("Nie udało się utworzyć administratora.");
            }
        }
    }
}