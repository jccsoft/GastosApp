# Configuración de Auth0 para Blazor WebAssembly

Esta guía explica cómo configurar la autenticación Auth0 en una aplicación Blazor WebAssembly standalone con .NET 9.

## 📋 Requisitos previos

- Cuenta de Auth0 activa
- Aplicación Blazor WebAssembly (.NET 9)
- Acceso al Auth0 Dashboard

## 🔧 Configuración en Auth0 Dashboard

### 1. Crear aplicación Single Page Application (SPA)

1. Ve al **Auth0 Dashboard**
2. Navega a **Applications** > **Create Application**
3. Selecciona **Single Page Web Applications**
4. Asigna un nombre descriptivo (ej: "Gastos PWA")

### 2. Configurar URLs permitidas

En la configuración de tu aplicación Auth0, configura las siguientes URLs:

```
Allowed Callback URLs:
https://localhost:7142/authentication/login-callback

Allowed Logout URLs:
https://localhost:7142/authentication/logout-callback
https://localhost:7142/authentication/logout-failed
https://localhost:7142/

Allowed Web Origins:
https://localhost:7142

Allowed Origins (CORS):
https://localhost:7142
```

**⚠️ Importante**: Para producción, reemplaza `https://localhost:7142` con tu dominio real.

### 3. Configuración avanzada

#### **Settings → Basic Information:**
- **Application Type**: Single Page Application
- **Token Endpoint Authentication Method**: None

#### **Settings → Advanced Settings → OAuth:**
- **JsonWebToken Signature Algorithm**: RS256
- **OIDC Conformant**: ✅ ENABLED

#### **Settings → Advanced Settings → Grant Types:**
- ✅ **Authorization Code** (ENABLED)
- ✅ **Refresh Token** (ENABLED)  
- ❌ **Implicit** (DISABLED - importante para seguridad)

## 📦 Paquetes NuGet necesarios

Agrega el siguiente paquete a tu proyecto Blazor WebAssembly:

```xml
<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Authentication" Version="9.0.0" />
```

## ⚙️ Configuración de la aplicación

### 1. Archivos de configuración

**Archivo**: `wwwroot/appsettings.json` (producción)

```json
{
  "Auth0": {
    "Authority": "https://TU-DOMINIO-AUTH0.auth0.com",
    "ClientId": "TU-CLIENT-ID",
    "ResponseType": "code",
    "Scope": "openid profile email"
  }
}
```

**Archivo**: `wwwroot/appsettings.Development.json` (desarrollo)

```json
{
  "Auth0": {
    "Authority": "https://dev-eyfr0qlg3zkooqow.eu.auth0.com",
    "ClientId": "AFlQyg8XEEJ358EX9BwYaFLAV6S6ps7x",
    "ResponseType": "code",
    "Scope": "openid profile email"
  }
}
```

### 2. Configuración de servicios de autenticación

**Archivo**: `DependencyInjection.cs` (configuración completa con logout mejorado)

```csharp
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
```

### 3. Script de autenticación

**Archivo**: `wwwroot/index.html`

Agrega este script antes del cierre del tag `</body>`:

```html
<script src="_content/Microsoft.AspNetCore.Components.WebAssembly.Authentication/AuthenticationService.js"></script>
```

### 4. Imports globales

**Archivo**: `Components/_Imports.razor`

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
@using Microsoft.JSInterop
@using Gastos.Pwa
@using Gastos.Pwa.Components
@using Gastos.Pwa.Components.Layout
@using Gastos.Pwa.Components.Pages
@using Gastos.Shared.Entities.Request
@using Gastos.Shared.Extensions
@using MudBlazor
```

## 🎯 Componentes de autenticación

### 1. Componente principal de la aplicación

**Archivo**: `Components/App.razor`

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized Context="authContext">
                    @if (authContext.User.Identity?.IsAuthenticated != true)
                    {
                        <RedirectToLogin />
                    }
                    else
                    {
                        <p role="alert">You are not authorized to access this resource.</p>
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

### 2. Componente de redirección

**Archivo**: `Components/RedirectToLogin.razor`

```razor
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
@inject NavigationManager Navigation
@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateToLogin("authentication/login");
    }
}
```

### 3. Páginas de autenticación (con MudBlazor)

#### Login
**Archivo**: `Components/Pages/Authentication/Login.razor`

```razor
@page "/authentication/login"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

<RemoteAuthenticatorView Action="@RemoteAuthenticationActions.LogIn">
    <LoggingIn>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
            <MudPaper Class="pa-8 text-center" Elevation="3">
                <MudProgressCircular Color="Color.Primary" Size="Size.Large" Indeterminate="true" Class="mb-4" />
                <MudText Typo="Typo.h5" Class="mb-2">Iniciando sesión</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">
                    Por favor, espere mientras se procesa su autenticación...
                </MudText>
            </MudPaper>
        </MudContainer>
    </LoggingIn>
</RemoteAuthenticatorView>
```

#### Logout
**Archivo**: `Components/Pages/Authentication/Logout.razor`

```razor
@page "/authentication/logout"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

<RemoteAuthenticatorView Action="@RemoteAuthenticationActions.LogOut">
    <LogOut>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
            <MudPaper Class="pa-8 text-center" Elevation="3">
                <MudProgressCircular Color="Color.Secondary" Size="Size.Large" Indeterminate="true" Class="mb-4" />
                <MudText Typo="Typo.h5" Class="mb-2">Cerrando sesión</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">
                    Por favor, espere mientras se cierra su sesión...
                </MudText>
            </MudPaper>
        </MudContainer>
    </LogOut>
    <LogOutFailed>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
            <MudPaper Class="pa-8 text-center" Elevation="3">
                <MudIcon Icon="@Icons.Material.Filled.Warning" Color="Color.Error" Size="Size.Large" Class="mb-4" />
                <MudText Typo="Typo.h5" Class="mb-2">Error al cerrar sesión</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary" Class="mb-4">
                    Ha ocurrido un error durante el cierre de sesión.
                </MudText>
                <MudButton Variant="Variant.Filled" 
                           Color="Color.Primary" 
                           StartIcon="@Icons.Material.Filled.Home"
                           Href="/">
                    Volver al inicio
                </MudButton>
            </MudPaper>
        </MudContainer>
    </LogOutFailed>
</RemoteAuthenticatorView>
```

#### Login Callback
**Archivo**: `Components/Pages/Authentication/LoginCallback.razor`

```razor
@page "/authentication/login-callback"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

<RemoteAuthenticatorView Action="@RemoteAuthenticationActions.LogInCallback">
    <CompletingLoggingIn>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
            <MudPaper Class="pa-8 text-center" Elevation="3">
                <MudProgressCircular Color="Color.Success" Size="Size.Large" Indeterminate="true" Class="mb-4" />
                <MudText Typo="Typo.h5" Class="mb-2">Completando autenticación</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">
                    Finalizando el proceso de inicio de sesión...
                </MudText>
            </MudPaper>
        </MudContainer>
    </CompletingLoggingIn>
</RemoteAuthenticatorView>
```

#### Logout Callback
**Archivo**: `Components/Pages/Authentication/LogoutCallback.razor`

```razor
@page "/authentication/logout-callback"
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication

<RemoteAuthenticatorView Action="@RemoteAuthenticationActions.LogOutCallback">
    <CompletingLogOut>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
            <MudPaper Class="pa-8 text-center" Elevation="3">
                <MudProgressCircular Color="Color.Success" Size="Size.Large" Indeterminate="true" Class="mb-4" />
                <MudText Typo="Typo.h5" Class="mb-2">Completando cierre de sesión</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary">
                    Finalizando el proceso de cierre de sesión...
                </MudText>
            </MudPaper>
        </MudContainer>
    </CompletingLogOut>
    <LogOutSucceeded>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
            <MudPaper Class="pa-8 text-center" Elevation="3">
                <MudIcon Icon="@Icons.Material.Filled.CheckCircle" Color="Color.Success" Size="Size.Large" Class="mb-4" />
                <MudText Typo="Typo.h5" Class="mb-2">Sesión cerrada exitosamente</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary" Class="mb-4">
                    Su sesión ha sido cerrada correctamente.
                </MudText>
                <MudButton Variant="Variant.Filled" 
                           Color="Color.Primary" 
                           StartIcon="@Icons.Material.Filled.Home"
                           Href="/">
                    Ir al inicio
                </MudButton>
            </MudPaper>
        </MudContainer>
    </LogOutSucceeded>
    <LogOutFailed>
        <MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
            <MudPaper Class="pa-8 text-center" Elevation="3">
                <MudIcon Icon="@Icons.Material.Filled.Error" Color="Color.Error" Size="Size.Large" Class="mb-4" />
                <MudText Typo="Typo.h5" Class="mb-2">Error en el callback de logout</MudText>
                <MudText Typo="Typo.body2" Color="Color.Secondary" Class="mb-4">
                    Error al procesar el callback de cierre de sesión.
                </MudText>
                <MudButton Variant="Variant.Filled" 
                           Color="Color.Primary" 
                           StartIcon="@Icons.Material.Filled.Home"
                           Href="/">
                    Volver al inicio
                </MudButton>
            </MudPaper>
        </MudContainer>
    </LogOutFailed>
</RemoteAuthenticatorView>
```

#### Login Failed
**Archivo**: `Components/Pages/Authentication/LoginFailed.razor`

```razor
@page "/authentication/login-failed"

<PageTitle>Error de autenticación</PageTitle>

<MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
    <MudPaper Class="pa-8 text-center" Elevation="3">
        <MudIcon Icon="@Icons.Material.Filled.Error" Color="Color.Error" Size="Size.Large" Class="mb-4" />
        <MudText Typo="Typo.h4" Class="mb-4">Error de autenticación</MudText>
        <MudText Typo="Typo.body1" Class="mb-6">
            Ha ocurrido un error durante el proceso de autenticación.
        </MudText>
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   StartIcon="@Icons.Material.Filled.Home"
                   Href="/">
            Volver al inicio
        </MudButton>
    </MudPaper>
</MudContainer>
```

#### Logout Failed
**Archivo**: `Components/Pages/Authentication/LogoutFailed.razor`

```razor
@page "/authentication/logout-failed"

<PageTitle>Error al cerrar sesión</PageTitle>

<MudContainer MaxWidth="MaxWidth.Medium" Class="d-flex justify-center align-center" Style="height: 100vh;">
    <MudPaper Class="pa-8 text-center" Elevation="3">
        <MudIcon Icon="@Icons.Material.Filled.Warning" Color="Color.Warning" Size="Size.Large" Class="mb-4" />
        <MudText Typo="Typo.h4" Class="mb-4">Error al cerrar sesión</MudText>
        <MudText Typo="Typo.body1" Class="mb-6">
            Ha ocurrido un error durante el proceso de cierre de sesión. Es posible que su sesión ya haya sido cerrada.
        </MudText>
        <MudStack Direction="Row" Justify="Justify.Center" Spacing="3">
            <MudButton Variant="Variant.Outlined" 
                       Color="Color.Primary" 
                       StartIcon="@Icons.Material.Filled.Refresh"
                       Href="/authentication/logout">
                Intentar de nuevo
            </MudButton>
            <MudButton Variant="Variant.Filled" 
                       Color="Color.Primary" 
                       StartIcon="@Icons.Material.Filled.Home"
                       Href="/">
                Volver al inicio
            </MudButton>
        </MudStack>
    </MudPaper>
</MudContainer>
```

### 4. Integración en la barra de navegación (.NET 9)

**Archivo**: `Components/Layout/AppBar.razor`

```razor
@using Microsoft.AspNetCore.Components.WebAssembly.Authentication
@inject NavigationManager Navigation

<MudAppBar Elevation="3" Dense>
    <AuthorizeView>
        <Authorized Context="authContext">
            <MudText Typo="Typo.subtitle1" Class="mr-2">
                @authContext.User.GetUserName()
            </MudText>
            <MudTooltip Text="Cerrar sesión">
                <MudIconButton Icon="@Icons.Material.Filled.Logout" 
                              Color="Color.Inherit" 
                              OnClick="BeginLogout" />
            </MudTooltip>
        </Authorized>
        <NotAuthorized>
            <MudText Typo="Typo.subtitle1" Class="mr-2">
                NO AUTORIZADO
            </MudText>
            <MudTooltip Text="Iniciar sesión">
                <MudIconButton Icon="@Icons.Material.Filled.Login" 
                              Color="Color.Inherit" 
                              Href="/authentication/login" />
            </MudTooltip>
        </NotAuthorized>
    </AuthorizeView>
</MudAppBar>

@code {
    private void BeginLogout()
    {
        // Enfoque recomendado para .NET 9
        Navigation.NavigateToLogout("/authentication/logout");
    }
}
```

## 🔐 URLs de autenticación estándar

La aplicación maneja automáticamente estas rutas:

- `/authentication/login` - Inicia el proceso de autenticación
- `/authentication/logout` - Inicia el proceso de cierre de sesión
- `/authentication/login-callback` - Callback después del login exitoso
- `/authentication/logout-callback` - Callback después del logout
- `/authentication/login-failed` - Página de error si falla el login
- `/authentication/logout-failed` - Página de error si falla el logout

## 🚀 Configuración para producción

Para desplegar en producción:

1. **Actualiza las URLs en Auth0** con tu dominio de producción
2. **Modifica `appsettings.json`** con los valores de producción
3. **Configura HTTPS** obligatorio para Auth0
4. **Configura variables de entorno** para los secrets si es necesario

## 🔍 Debugging y Troubleshooting

### Errores comunes:

1. **Error 401**: Verifica que el Client ID sea correcto y que la aplicación sea tipo SPA
2. **CORS errors**: Asegúrate de que el dominio esté configurado en "Allowed Origins (CORS)"
3. **Redirect mismatch**: Verifica que las URLs de callback coincidan exactamente
4. **Logout no funciona**: Verifica que uses `NavigationManager.NavigateToLogout()` en .NET 9
5. **URLs relativas**: Asegúrate de usar `/authentication/logout` (con barra inicial)

### Para depurar:

1. Abre Developer Tools (F12)
2. Ve a la pestaña Network
3. Observa las requests a Auth0 durante el login/logout
4. Revisa la consola para errores específicos

### Página de debug (opcional):

Puedes crear una página `/debug-auth` para monitorear el estado de autenticación:

```razor
@page "/debug-auth"
@using Microsoft.AspNetCore.Components.Authorization
@inject AuthenticationStateProvider AuthenticationStateProvider

<h3>Estado de Autenticación - Debug</h3>
<!-- Implementación completa en el código del proyecto -->
```

## ✅ Checklist de verificación

- [ ] Aplicación Auth0 configurada como SPA
- [ ] Grant Type "Implicit" DESHABILITADO en Auth0
- [ ] URLs de callback configuradas (incluyendo logout-failed)
- [ ] Paquete NuGet instalado
- [ ] Script de autenticación agregado al index.html
- [ ] Configuración en appsettings.json y appsettings.Development.json
- [ ] Servicios registrados en DependencyInjection
- [ ] Componentes de autenticación creados con MudBlazor
- [ ] App.razor configurado con AuthorizeRouteView
- [ ] Imports globales actualizados
- [ ] AppBar usando NavigationManager.NavigateToLogout() para .NET 9
- [ ] Todas las URLs usando rutas absolutas (/)

## 🎯 Notas importantes para .NET 9

- ✅ Usar `NavigationManager.NavigateToLogout()` en lugar de `SignOutSessionStateManager` (obsoleto)
- ✅ Grant Type "Implicit" debe estar DESHABILITADO en Auth0
- ✅ Todas las URLs deben ser absolutas (comenzar con `/`)
- ✅ Configurar rutas de logout explícitamente en `AuthenticationPaths`

Con esta configuración completa, tu aplicación Blazor WebAssembly debería autenticarse correctamente con Auth0 usando las mejores prácticas para .NET 9.