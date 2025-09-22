using Gastos.Shared.Options;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Gastos.Pwa.Shared.Extensions;

public static class AuthExtensions
{
    public static WebAssemblyHostBuilder AddAuthServices(this WebAssemblyHostBuilder builder)
    {
        string auth0Section = Auth0Options.ConfigurationSection;
        Auth0Options? authOptions = builder.Configuration.GetSection(auth0Section).Get<Auth0Options>();
        string audience = authOptions?.Audience ?? "";
        string baseAddress = builder.HostEnvironment.BaseAddress;

        builder.Services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState();

        builder.Services
            .AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind(auth0Section, options.ProviderOptions);

                options.ProviderOptions.RedirectUri = $"{baseAddress}authentication/login-callback";
                options.ProviderOptions.PostLogoutRedirectUri = $"{baseAddress}authentication/logout-callback";

                options.ProviderOptions.ResponseType = "code";

                options.ProviderOptions.DefaultScopes.Clear();
                options.ProviderOptions.DefaultScopes.Add("openid");
                options.ProviderOptions.DefaultScopes.Add("profile");
                options.ProviderOptions.DefaultScopes.Add("email");
                options.ProviderOptions.DefaultScopes.Add(audience);

                options.ProviderOptions.AdditionalProviderParameters.Add("federated", "");
                options.ProviderOptions.AdditionalProviderParameters.Add("audience", audience);
            });

        builder.Services
            .AddTransient(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<BearerTokenHttpHandler>>();
                var tokenProvider = sp.GetRequiredService<IAccessTokenProvider>();

                return new BearerTokenHttpHandler(tokenProvider, logger);
            });

        return builder;
    }
}
