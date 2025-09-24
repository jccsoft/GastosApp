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

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // Allow any origin in development
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    // Production: only allow specific origins
                    policy.WithOrigins(
                        "https://thankful-desert-0e532df03.1.azurestaticapps.net",
                        "https://jcdcgastosapi.azurewebsites.net"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                }
            });
        });

        builder
            .AddAuthServices()
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