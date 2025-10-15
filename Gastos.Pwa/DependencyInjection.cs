using FluentValidation;
using Gastos.Pwa.Camera;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Gastos.Pwa;

public static class DependencyInjection
{
    public static WebAssemblyHostBuilder AddMyPwaServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services
            .AddLocalizationServices()
            .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped)
            .AddValidatorsFromAssemblyContaining<Gastos.Shared.IApplicationMarker>(ServiceLifetime.Scoped)
            .AddScoped<StateContainer>()
            .AddScoped<BlazorService>()
            .AddScoped<ThemeService>()
            .AddScoped<PwaUpdateService>()
            .AddScoped<INetworkStatusService, NetworkStatusService>()
            .AddScoped<IVersionService, VersionService>()
            .AddScoped<ICameraService, CameraService>();

        builder.AddAuthServices();

        string apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "";
        builder.Services.AddRefitClients(apiBaseAddress);

        return builder;
    }
}
