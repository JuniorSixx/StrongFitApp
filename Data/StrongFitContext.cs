using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StrongFitApp.Models;

namespace StrongFitApp.Data
{
    public class StrongFitContext : IdentityDbContext
    {
        public StrongFitContext(DbContextOptions<StrongFitContext> options)
            : base(options)
        {
        }

        public DbSet<Personal> Personals { get; set; }
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Exercicio> Exercicios { get; set; }
        public DbSet<Treino> Treinos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // relacionamento muitos-para-muitos entre Treino e Exercicio
            modelBuilder.Entity<Treino>()
                .HasMany(t => t.Exercicios)
                .WithMany(e => e.Treinos)
                .UsingEntity(j => j.ToTable("TreinoExercicio"));

            //relacionamento entre Aluno e Personal
            modelBuilder.Entity<Aluno>()
                .HasOne(a => a.Personal)
                .WithMany(p => p.Alunos)
                .HasForeignKey(a => a.PersonalID);

            // relacionamento entre Treino e Aluno
            modelBuilder.Entity<Treino>()
                .HasOne(t => t.Aluno)
                .WithMany(a => a.Treinos)
                .HasForeignKey(t => t.AlunoID);
        }
    }
}
