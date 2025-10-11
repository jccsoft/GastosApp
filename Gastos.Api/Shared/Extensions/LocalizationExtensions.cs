using Microsoft.AspNetCore.Localization;
using static Gastos.Shared.Resources.LocalizationConstants;
namespace Gastos.Api.Shared.Extensions;

public static class LocalizationExtensions
{
    public static WebApplicationBuilder AddLocalizationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddLocalization(options => options.ResourcesPath = ResourcesPath);
        builder.Services.AddScoped<LocalizationService>();

        return builder;
    }

    public static void UseMyRequestLocalization(this WebApplication app)
    {
        CultureInfo[]? supportedCultures = [.. SupportedCultures.Cultures.Select(c => new CultureInfo(c.Key))];

        var options = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(SupportedCultures.DefaultCultureKey),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        };

        app.UseRequestLocalization(options);
    }
}
