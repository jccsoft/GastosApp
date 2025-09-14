using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Gastos.Api.Shared.Extensions;

public static class Auth0Extensions
{
    public static WebApplicationBuilder AddAuth0Services(this WebApplicationBuilder builder)
    {
        var authOptions = builder.Configuration.GetSection(Auth0Options.ConfigurationSection).Get<Auth0Options>();

        builder.Services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = authOptions!.Domain;
            options.ClientId = authOptions!.ClientId;
        });

        return builder;
    }

    public static void MapAuth0Endpoints(this WebApplication app)
    {
        app.MapGet("account/login", async (HttpContext context, string returnUrl = "/") =>
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                 .WithRedirectUri(returnUrl)
                 .Build();
            await context.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        });


        app.MapGet("account/logout", async (context) =>
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                 .WithRedirectUri("/")
                 .Build();
            await context.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        });
    }

}
