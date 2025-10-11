using Gastos.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gastos.Api.Shared.Extensions;

/// <summary>
/// Provides extension methods for configuring authentication services.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication services to the application using Auth0 configuration.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplicationBuilder"/> for method chaining.</returns>
    /// <remarks>
    /// This method configures JWT Bearer authentication with Auth0, including:
    /// <list type="bullet">
    /// <item><description>Setting the authority and audience from Auth0 configuration</description></item>
    /// <item><description>Configuring token validation parameters with strict validation rules</description></item>
    /// <item><description>Setting clock skew to zero for precise token expiration validation</description></item>
    /// <item><description>Adding authorization services</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when Auth0 configuration is missing or invalid.</exception>
    public static WebApplicationBuilder AddAuthServices(this WebApplicationBuilder builder)
    {
        var authOptions = builder.Configuration.GetSection(Auth0Options.ConfigurationSection).Get<Auth0Options>();
        string authority = $"https://{authOptions!.Domain}/";
        string audience = authOptions.Audience;

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = authority,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}
