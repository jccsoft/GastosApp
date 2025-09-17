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
            .AddScoped<ThemeService>();

        // Configuración OIDC para Auth0
        builder.Services.AddOidcAuthentication(options =>
        {
            // Usar configuración del archivo appsettings.json
            builder.Configuration.Bind("Auth0", options.ProviderOptions);
            
            // Configurar URLs de redirección dinámicamente
            var baseAddress = builder.HostEnvironment.BaseAddress;
            options.ProviderOptions.RedirectUri = $"{baseAddress}authentication/login-callback";
            options.ProviderOptions.PostLogoutRedirectUri = $"{baseAddress}authentication/logout-callback";
            
            // Configuración específica para Auth0
            options.ProviderOptions.ResponseType = "code";
            
            // Configurar scopes específicos
            options.ProviderOptions.DefaultScopes.Clear();
            options.ProviderOptions.DefaultScopes.Add("openid");
            options.ProviderOptions.DefaultScopes.Add("profile");
            options.ProviderOptions.DefaultScopes.Add("email");
            
            // Configuración específica para Auth0 logout
            options.ProviderOptions.AdditionalProviderParameters.Add("federated", "");
            
            // Configuración de rutas de autenticación
            options.AuthenticationPaths.LogOutPath = "authentication/logout";
            options.AuthenticationPaths.LogOutCallbackPath = "authentication/logout-callback";
            options.AuthenticationPaths.LogOutFailedPath = "authentication/logout-failed";
        });

        builder.Services.AddRefitClients(
            baseUrl: builder.HostEnvironment.BaseAddress,
            addHandler: false);

        return builder;
    }
}