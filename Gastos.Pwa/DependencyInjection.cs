using FluentValidation;
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
            options.ProviderOptions.DefaultScopes.Add("https://gastos-api"); // ✅ Agregar el audience como scope

            options.ProviderOptions.AdditionalProviderParameters.Add("federated", "");
            options.ProviderOptions.AdditionalProviderParameters.Add("audience", "https://gastos-api"); // ✅ Especificar audience
        });


        builder.Services.AddTransient<BearerTokenHttpHandler>();

        var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "";
        builder.Services.AddRefitClients(
            baseUrl: apiBaseAddress,
            addHandler: true);

        return builder;
    }
}