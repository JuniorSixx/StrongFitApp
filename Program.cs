using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StrongFitApp.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.Google; // 👈 necessário
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<StrongFitContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StrongFitContext>();

// ✅ CONFIGURA LOGIN COM GOOGLE
builder.Services.AddAuthentication()
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var dbContext = services.GetRequiredService<StrongFitContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var sql = @"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Exercicios]') AND name = 'Series')
            BEGIN
                ALTER TABLE [dbo].[Exercicios]
                ADD [Series] INT NOT NULL DEFAULT 3
            END

            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Exercicios]') AND name = 'Repeticoes')
            BEGIN
                ALTER TABLE [dbo].[Exercicios]
                ADD [Repeticoes] INT NOT NULL DEFAULT 12
            END
        ";

        await dbContext.Database.ExecuteSqlRawAsync(sql);
        logger.LogInformation("Script SQL para adicionar colunas executado com sucesso.");

        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.InitializeAsync(dbContext, userManager, roleManager);

        logger.LogInformation("Banco de dados inicializado com sucesso.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocorreu um erro durante a inicialização do banco de dados.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ esses dois são obrigatórios
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
