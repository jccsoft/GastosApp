using Gastos.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gastos.Api.Shared.Extensions;

public static class Auth0Extensions
{
    public static WebApplicationBuilder AddAuth0Services(this WebApplicationBuilder builder)
    {
        var authOptions = builder.Configuration.GetSection(Auth0Options.ConfigurationSection).Get<Auth0Options>();


        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Configuración para Auth0
                options.Authority = $"https://{authOptions!.Domain}/";
                options.Audience = authOptions.Audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = $"https://{authOptions.Domain}/",
                    ValidAudience = authOptions.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        return builder;
    }
}
