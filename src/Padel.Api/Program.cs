using Microsoft.EntityFrameworkCore;
using Padel.Application.Interfaces;
using Padel.Application.Services;
using Padel.Infrastructure.Data;
using Padel.Infrastructure.Jobs;
using Padel.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7271", "http://localhost:5073")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// CF-AA-001: Use padel_app_user in production. Falls back to DefaultConnection for dev (LocalDB).
var connectionString = builder.Configuration.GetConnectionString("PadelAppUser")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<PadelDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ISiteRepository, SiteRepository>();
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<ICourtRepository, CourtRepository>();
builder.Services.AddScoped<ICourtService, CourtService>();
builder.Services.AddScoped<ISiteScheduleRepository, SiteScheduleRepository>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IClosureDayRepository, ClosureDayRepository>();
builder.Services.AddScoped<IClosureDayService, ClosureDayService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddHostedService<DayBeforeMatchJob>();

var app = builder.Build();

// =============================================
// Initialisation automatique de la base de données
// =============================================
var autoSeed = builder.Configuration.GetValue<bool>("Database:AutoSeedData", true);

if (autoSeed)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<PadelDbContext>();

            // Créer la base de données si elle n'existe pas
            await context.Database.EnsureCreatedAsync();

            // Initialiser les données de démonstration si la base est vide
            var seeder = new DatabaseSeeder(context);
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "❌ Une erreur s'est produite lors de l'initialisation de la base de données.");
        }
    }
}

// Configure the HTTP request pipeline.

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
