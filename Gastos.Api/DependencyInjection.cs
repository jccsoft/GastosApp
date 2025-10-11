namespace Gastos.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddMyApiServices(this WebApplicationBuilder builder)
    {
        builder
            .AddOptionsServices()
            .AddValidatorServices()
            .AddCorsPolicy()
            .AddAuthServices()
            .AddDatabaseServices()
            .AddRepoServices()
            .AddLocalizationServices()
            .AddTelemetryServices(); // Agregamos configuración de telemetría

        builder.Services
            .AddOpenApi() // For Swagger/OpenAPI support
            .AddHttpContextAccessor() // To access HttpContext in services
            .AddHttpForwarder(); // For reverse proxying

        return builder;
    }

    private static WebApplicationBuilder AddTelemetryServices(this WebApplicationBuilder builder)
    {
        // Configurar logging detallado para debug
        builder.Logging.Configure(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.ParentId |
                ActivityTrackingOptions.Baggage |
                ActivityTrackingOptions.Tags;
        });

        // Agregar logging estructurado con mayor detalle en producción
        if (builder.Environment.IsProduction())
        {
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Information);
            builder.Logging.AddFilter("Gastos.Api", LogLevel.Information);
        }
        else
        {
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            builder.Logging.AddFilter("Gastos.Api", LogLevel.Debug);
        }

        return builder;
    }
}