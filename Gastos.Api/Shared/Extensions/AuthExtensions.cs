using Gastos.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Gastos.Api.Shared.Extensions;

public static class AuthExtensions
{
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
