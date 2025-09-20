using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace Gastos.Pwa.Shared.Services;

public class BearerTokenHttpHandler(IAccessTokenProvider tokenProvider, ILogger<BearerTokenHttpHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Agregando token de acceso a la request: {RequestUri}", request.RequestUri);
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

                // Para requests que no requieren autenticación, continuar sin token
                // Para requests que SÍ requieren autenticación, el servidor responderá con 401
                // y el componente que haga la llamada puede manejar el error apropiadamente
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error agregando token de acceso a la request: {RequestUri}", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
