using Gastos.Pwa;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();

builder.AddMyPwaServices();

// ✅ Configurar logging detallado
builder.Logging
    .SetMinimumLevel(LogLevel.Debug)
    .AddFilter("Microsoft.AspNetCore.Components.WebAssembly.Authentication", LogLevel.Debug)
    .AddFilter("System.Net.Http", LogLevel.Debug);

await builder.Build().RunAsync();
