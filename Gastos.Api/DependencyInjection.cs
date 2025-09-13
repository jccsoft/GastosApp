using static Gastos.Shared.Resources.LocalizationConstants;

namespace Gastos.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<GastosApiOptions>().BindConfiguration(GastosApiOptions.ConfigurationSection).ValidateOnStart();
        builder.Services
            .AddOptions<DocIntelApiOptions>().BindConfiguration(DocIntelApiOptions.ConfigurationSection).ValidateOnStart();

        builder.Services
            .AddLocalization(options => options.ResourcesPath = ResourcesPath)
            .AddScoped<LocalizationService>();

        builder.Services
            .AddValidatorsFromAssemblyContaining<Gastos.Shared.IApplicationMarker>(ServiceLifetime.Scoped)
            .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

        builder
            .AddDatabaseServices()
            .AddRepoServices();

        builder.Services
            .AddHttpContextAccessor()
            .AddHttpForwarder();

        return builder;
    }
}