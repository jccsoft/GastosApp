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
            .AddTelemetryServices() // Agregamos configuración de telemetría
            .LogApplicationInsightsConfiguration(); // Log Application Insights configuration

        builder.Services
            .AddOpenApi() // For Swagger/OpenAPI support
            .AddHttpContextAccessor() // To access HttpContext in services
            .AddHttpForwarder(); // For reverse proxying

        return builder;
    }

    private static WebApplicationBuilder LogApplicationInsightsConfiguration(this WebApplicationBuilder builder)
    {
        // Log Application Insights configuration status
        var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] 
                             ?? builder.Configuration["AzureMonitor:ConnectionString"];

        if (!string.IsNullOrEmpty(connectionString))
        {
            // Log that Application Insights is configured (without exposing the full connection string)
            var maskedConnectionString = connectionString.Length > 20 
                ? $"{connectionString[..20]}...{connectionString[^10..]}" 
                : "***";
            
            Console.WriteLine($"✅ Application Insights configured with connection string: {maskedConnectionString}");
        }
        else
        {
            Console.WriteLine("⚠️  Application Insights not configured - no connection string found");
            Console.WriteLine("   Add APPLICATIONINSIGHTS_CONNECTION_STRING environment variable or AzureMonitor:ConnectionString in appsettings.json");
        }

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
            
            // Log Azure Monitor configuration
            builder.Logging.AddFilter("Azure.Monitor", LogLevel.Information);
            builder.Logging.AddFilter("OpenTelemetry", LogLevel.Information);
        }
        else
        {
            builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
            builder.Logging.AddFilter("Gastos.Api", LogLevel.Debug);
            
            // In development, show more Azure Monitor details
            builder.Logging.AddFilter("Azure.Monitor", LogLevel.Debug);
            builder.Logging.AddFilter("OpenTelemetry", LogLevel.Debug);
        }

        return builder;
    }
}