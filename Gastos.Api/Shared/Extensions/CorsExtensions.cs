namespace Gastos.Api.Shared.Extensions;

/// <summary>
/// Provides extension methods for configuring Cross-Origin Resource Sharing (CORS) policies.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Configures CORS policies for the web application with environment-specific settings.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure CORS for.</param>
    /// <returns>The configured <see cref="WebApplicationBuilder"/> for method chaining.</returns>
    /// <remarks>
    /// In development environments, allows any origin, method, and header for easier testing.
    /// In production environments, restricts access to specific trusted origins and enables credentials.
    /// </remarks>
    public static WebApplicationBuilder AddMyCorsPolicy(this WebApplicationBuilder builder)
    {
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
                        "https://jcdcgastosapi.azurewebsites.net",
                        "https://localhost:7142"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                }
            });
        });

        return builder;
    }
}
