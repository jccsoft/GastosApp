using Gastos.Shared.Resources;
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

    //public static void UseMyRequestLocalization(this WebAssemblyHost app)
    //{
    //    CultureInfo[]? supportedCultures = [.. SupportedCultures.Cultures.Select(c => new CultureInfo(c.Key))];

    //    var options = new RequestLocalizationOptions
    //    {
    //        DefaultRequestCulture = new RequestCulture(SupportedCultures.DefaultCultureKey),
    //        SupportedCultures = supportedCultures,
    //        SupportedUICultures = supportedCultures
    //    };

    //    app.UseRequestLocalization(options);
    //}
}
