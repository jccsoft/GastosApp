namespace Gastos.Api.Shared.Extensions;

/// <summary>
/// Provides extension methods for configuring application options during application startup.
/// </summary>
public static class OptionsExtensions
{
    /// <summary>
    /// Configures and registers application-specific options with the dependency injection container.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> instance to configure.</param>
    /// <returns>The configured <see cref="WebApplicationBuilder"/> instance for method chaining.</returns>
    /// <remarks>
    /// This method registers the following options:
    /// <list type="bullet">
    /// <item><description><see cref="GastosApiOptions"/> - Bound from the configuration section defined in <see cref="GastosApiOptions.ConfigurationSection"/></description></item>
    /// <item><description><see cref="DocIntelApiOptions"/> - Bound from the configuration section defined in <see cref="DocIntelApiOptions.ConfigurationSection"/></description></item>
    /// </list>
    /// All options are validated on application start to ensure configuration integrity.
    /// </remarks>
    public static WebApplicationBuilder AddOptionsServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<GastosApiOptions>().BindConfiguration(GastosApiOptions.ConfigurationSection).ValidateOnStart();

        builder.Services
            .AddOptions<DocIntelApiOptions>().BindConfiguration(DocIntelApiOptions.ConfigurationSection).ValidateOnStart();

        return builder;
    }
}
