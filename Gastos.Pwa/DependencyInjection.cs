using FluentValidation;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Gastos.Pwa.Services;

namespace Gastos.Pwa;

public static class DependencyInjection
{
    public static WebAssemblyHostBuilder AddMyPwaServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services
            .AddLocalizationServices()
            .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped)
            .AddScoped<StateContainer>()
            .AddScoped<BlazorService>()
            .AddScoped<ThemeService>()
            .AddScoped<PwaUpdateService>()
            .AddScoped<INetworkStatusService, NetworkStatusService>()
            .AddScoped<IVersionService, VersionService>();

        builder.AddAuthServices();

        string apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "";
        builder.Services.AddRefitClients(apiBaseAddress);

        return builder;
    }
}
