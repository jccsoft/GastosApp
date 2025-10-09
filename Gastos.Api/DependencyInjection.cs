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

        return builder;
    }
}