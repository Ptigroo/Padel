using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Padel.Frontend;
using Padel.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7021") });
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<ICourtService, CourtService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IClosureDayService, ClosureDayService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddSingleton<AdminSessionService>();

await builder.Build().RunAsync();
