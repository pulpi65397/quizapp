using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Hubs;
using QuizApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Dodaj DbContext do usługi
builder.Services.AddDbContext<QuizAppContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("QuizAppContext")));

// Dodaj usługi tożsamości
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<QuizAppContext>()
    .AddDefaultTokenProviders();

// Dodaj kontrolery i widoki
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache(); // Dodajemy buforowanie w pamięci (opcjonalne, ale zalecane)

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Ustawiamy czas wygaśnięcia sesji (opcjonalne)
    options.Cookie.HttpOnly = true; // Zapewniamy, że cookie sesji jest dostępne tylko po stronie serwera (zalecane ze względów bezpieczeństwa)
    options.Cookie.IsEssential = true; // Opcjonalne: Czy sesja jest niezbędna do działania aplikacji
});
builder.Services.AddSignalR();

var app = builder.Build();

// Skonfiguruj potok żądań
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication(); // Dodaj uwierzytelnianie
app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    // Dodaj endpointy dla konta, jeśli masz oddzielny kontroler
    endpoints.MapControllerRoute(
        name: "account",
        pattern: "Account/{action=Login}/{id?}",
        defaults: new { controller = "Account", action = "Login" });
    endpoints.MapHub<QuizHub>("/quizHub");
});

app.UseCors(builder => builder
    .WithOrigins("http://localhost:3000") // <-- Dodaj adres twojej domeny klienckiej
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()); // Jeśli używasz uwierzytelniania

app.Run();
