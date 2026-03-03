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

await builder.Build().RunAsync();
