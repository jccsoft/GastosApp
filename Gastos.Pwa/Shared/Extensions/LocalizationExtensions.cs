using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using static Gastos.Shared.Resources.LocalizationConstants;

namespace Gastos.Pwa.Shared.Extensions;

public static class LocalizationExtensions
{
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = ResourcesPath);
        services.AddScoped<LocalizationService>();

        return services;
    }

    public static async Task InitializeCultureAsync(this WebAssemblyHost host)
    {
        var js = host.Services.GetRequiredService<IJSRuntime>();

        var cultureKey = await js.InvokeAsync<string>(GetItemFunction, CultureKey);

        if (string.IsNullOrWhiteSpace(cultureKey) || SupportedCultures.Cultures.All(c => c.Key != cultureKey))
        {
            cultureKey = SupportedCultures.DefaultCultureKey;
        }

        var defaultCulture = new CultureInfo(cultureKey);
        CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
        CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;
    }
}
