using FluentValidation;
using Gastos.Shared.Options;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Gastos.Pwa;

public static class DependencyInjection
{
    public static WebAssemblyHostBuilder AddMyPwaServices(this WebAssemblyHostBuilder builder)
    {
        builder.Services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState();

        builder.Services
            .AddLocalizationServices()
            .AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Scoped)
            .AddScoped<StateContainer>()
            .AddScoped<BlazorService>()
            .AddScoped<ThemeService>()
            .AddScoped<INetworkStatusService, NetworkStatusService>();


        // Configuración OIDC para Auth0
        var authOptions = builder.Configuration
            .GetSection(Auth0Options.ConfigurationSection).Get<Auth0Options>();

        builder.Services.AddOidcAuthentication(options =>
        {
            // Usar configuración del archivo appsettings.json
            builder.Configuration.Bind("Auth0", options.ProviderOptions);

            // Configurar URLs de redirección
            options.ProviderOptions.RedirectUri = $"{builder.HostEnvironment.BaseAddress}authentication/login-callback";
            options.ProviderOptions.PostLogoutRedirectUri = $"{builder.HostEnvironment.BaseAddress}authentication/logout-callback";

            options.ProviderOptions.ResponseType = "code";

            // Configurar scopes (incluir el audience como scope)
            options.ProviderOptions.DefaultScopes.Clear();
            options.ProviderOptions.DefaultScopes.Add("openid");
            options.ProviderOptions.DefaultScopes.Add("profile");
            options.ProviderOptions.DefaultScopes.Add("email");
            options.ProviderOptions.DefaultScopes.Add(authOptions!.Audience);

            options.ProviderOptions.AdditionalProviderParameters.Add("federated", "");
            options.ProviderOptions.AdditionalProviderParameters.Add("audience", authOptions!.Audience);
        });



        var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "";

        builder.Services.AddTransient(sp =>
        {
            //var authHandler = sp.GetRequiredService<AuthorizationMessageHandler>();
            var logger = sp.GetRequiredService<ILogger<BearerTokenHttpHandler>>();
            var tokenProvider = sp.GetRequiredService<IAccessTokenProvider>();

            //authHandler.ConfigureHandler(
            //    authorizedUrls: [apiBaseAddress],
            //    scopes: [authOptions!.Audience]);

            return new BearerTokenHttpHandler(tokenProvider, logger);
        });

        builder.Services.AddRefitClients(apiBaseAddress);

        return builder;
    }
}
