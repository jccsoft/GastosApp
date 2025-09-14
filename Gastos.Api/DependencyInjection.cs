namespace Gastos.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddMyApiServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<GastosApiOptions>().BindConfiguration(GastosApiOptions.ConfigurationSection).ValidateOnStart();
        builder.Services
            .AddOptions<DocIntelApiOptions>().BindConfiguration(DocIntelApiOptions.ConfigurationSection).ValidateOnStart();

        builder.Services
            .AddValidatorsFromAssemblyContaining<Gastos.Shared.IApplicationMarker>(ServiceLifetime.Scoped)
            .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped);

        builder
            .AddDatabaseServices()
            .AddRepoServices();

        builder.Services
            .AddOpenApi()
            .AddLocalizationServices()
            .AddHttpContextAccessor()
            .AddHttpForwarder();

        return builder;
    }
}