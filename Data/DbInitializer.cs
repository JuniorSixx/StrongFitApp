using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StrongFitApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrongFitApp.Data
{
    public static class DbInitializer
    {

        public static async Task InitializeAsync(StrongFitContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await context.Database.EnsureCreatedAsync();

            await InitializeDatabaseAsync(context);

            string[] roleNames = { "Admin", "Personal", "Aluno" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminEmail = "admin@strongfit.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (!context.Personals.Any())
            {
                var personalEmail = "personal@strongfit.com";
                var personalUser = await userManager.FindByEmailAsync(personalEmail);

                if (personalUser == null)
                {
                    personalUser = new IdentityUser
                    {
                        UserName = personalEmail,
                        Email = personalEmail,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(personalUser, "Personal@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(personalUser, "Personal");

                        var personal = new Personal
                        {
                            Nome = "João Silva",
                            Email = personalEmail,
                            Especialidade = "Musculação",
                            Telefone = "(11) 98765-4321",
                            Instagram = "@joao.personal"
                        };

                        await context.Personals.AddAsync(personal);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public static async Task InitializeDatabaseAsync(StrongFitContext context)
        {
            if (!context.Exercicios.Any())
            {
                var exercicios = new List<Exercicio>
                {
                    new Exercicio
                    {
                        Nome = "Supino Reto",
                        Descricao = "Deite-se em um banco reto, segure a barra com as mãos um pouco mais afastadas que a largura dos ombros, e empurre a barra para cima.",
                        Categoria = "Peito",
                        Series = 4,
                        Repeticoes = 12
                    },
                    new Exercicio
                    {
                        Nome = "Agachamento",
                        Descricao = "Posicione-se com os pés na largura dos ombros, dobre os joelhos e abaixe o corpo como se fosse sentar em uma cadeira.",
                        Categoria = "Pernas",
                        Series = 4,
                        Repeticoes = 15
                    },
                    new Exercicio
                    {
                        Nome = "Puxada Frontal",
                        Descricao = "Sente-se na máquina, segure a barra com as mãos afastadas e puxe-a para baixo até a altura do queixo.",
                        Categoria = "Costas",
                        Series = 3,
                        Repeticoes = 12
                    },
                    new Exercicio
                    {
                        Nome = "Desenvolvimento de Ombros",
                        Descricao = "Sentado ou em pé, segure os halteres na altura dos ombros e empurre-os para cima.",
                        Categoria = "Ombros",
                        Series = 3,
                        Repeticoes = 10
                    },
                    new Exercicio
                    {
                        Nome = "Rosca Direta",
                        Descricao = "Em pé, segure a barra com as palmas das mãos voltadas para cima e dobre os cotovelos.",
                        Categoria = "Braços",
                        Series = 3,
                        Repeticoes = 12
                    },
                    new Exercicio
                    {
                        Nome = "Abdominal Crunch",
                        Descricao = "Deite-se de costas, dobre os joelhos, coloque as mãos atrás da cabeça e levante o tronco.",
                        Categoria = "Abdômen",
                        Series = 3,
                        Repeticoes = 20
                    },
                    new Exercicio
                    {
                        Nome = "Esteira",
                        Descricao = "Caminhe ou corra na esteira por um período determinado.",
                        Categoria = "Cardio",
                        Series = 1,
                        Repeticoes = 30
                    },
                    new Exercicio
                    {
                        Nome = "Burpee",
                        Descricao = "Comece em pé, agache, coloque as mãos no chão, estique as pernas para trás, volte à posição de agachamento e salte.",
                        Categoria = "Funcional",
                        Series = 3,
                        Repeticoes = 15
                    },
                    new Exercicio
                    {
                        Nome = "Alongamento de Isquiotibiais",
                        Descricao = "Sente-se no chão com as pernas estendidas e tente alcançar os pés com as mãos.",
                        Categoria = "Alongamento",
                        Series = 3,
                        Repeticoes = 30
                    },
                    new Exercicio
                    {
                        Nome = "Leg Press",
                        Descricao = "Sente-se na máquina, coloque os pés na plataforma e empurre-a para cima e para baixo.",
                        Categoria = "Pernas",
                        Series = 4,
                        Repeticoes = 12
                    }
                };

                await context.Exercicios.AddRangeAsync(exercicios);
                await context.SaveChangesAsync();
            }
        }
    }
}
