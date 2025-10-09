using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Trace;

namespace Gastos.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddMyApiServices(this WebApplicationBuilder builder)
    {
        builder
            .AddMyOptions()
            .AddMyValidators()
            .AddMyCors()
            .AddMyAuthServices()
            .AddMyDatabaseServices()
            .AddMyRepoServices()
            .AddMyLocalizationServices();

        builder.Services
            .AddOpenApi() // For Swagger/OpenAPI support
            .AddHttpContextAccessor() // To access HttpContext in services
            .AddHttpForwarder(); // For reverse proxying


        if (builder.Environment.IsDevelopment())
        {
            builder.Services
                .AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:4317");
                }));
        }
        else
        {
            builder.Services
                .AddOpenTelemetry()
                .UseAzureMonitor();
        }

        return builder;
    }
}