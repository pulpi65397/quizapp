using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.Data
{
    public class QuizAppContext : IdentityDbContext<ApplicationUser>
    {
        public QuizAppContext (DbContextOptions<QuizAppContext> options)
            : base(options)
        {
        }

        public DbSet<QuizApp.Models.Quiz> Quiz { get; set; } = default!;

        public DbSet<QuizApp.Models.Pytanie> Pytanie { get; set; } = default!;

        public DbSet<QuizApp.Models.Odpowiedz> Odpowiedz { get; set; } = default!;

        public DbSet<QuizApp.Models.Wynik> Wynik { get; set; } = default!;

        public DbSet<QuizApp.Models.QuizToken> QuizToken { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Pytanie>()
                .HasOne(p => p.Quiz)
                .WithMany(q => q.Pytania)  
                .HasForeignKey(p => p.QuizId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Odpowiedz>()
                .HasOne(o => o.Pytanie)
                .WithMany(p => p.Odpowiedzi) 
                .HasForeignKey(o => o.PytanieId);

        }


        public DbSet<QuizApp.Models.Uzytkownik> Uzytkownik { get; set; } = default!;
    }
}
