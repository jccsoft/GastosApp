using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace Gastos.Pwa.Shared.Services;

public class BearerTokenHttpHandler(
    IAccessTokenProvider tokenProvider,
    ILogger<BearerTokenHttpHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("🚀 Request: {RequestUri}", request.RequestUri);

            // Intentar obtener el token de acceso
            var tokenResult = await tokenProvider.RequestAccessToken();

            if (tokenResult.TryGetToken(out var token))
            {
                // Agregar el token Bearer al header de autorización
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
                logger.LogDebug("Token de acceso agregado a la request: {RequestUri}", request.RequestUri);
            }
            else
            {
                logger.LogWarning("No se pudo obtener el token de acceso para la request: {RequestUri}. Status: {Status}",
                    request.RequestUri, tokenResult.Status);
            }

            var response = await base.SendAsync(request, cancellationToken);

            logger.LogInformation("✅ Response: {StatusCode}", response.StatusCode);

            return response;
        }
        catch (AccessTokenNotAvailableException ex)
        {
            logger.LogWarning(ex, "🔐 Token no disponible, redirigiendo al login");
            ex.Redirect();
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "💥 Error en request");
            throw;
        }
    }
}