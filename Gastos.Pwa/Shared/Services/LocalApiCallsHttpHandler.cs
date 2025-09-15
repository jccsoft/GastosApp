using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Gastos.Pwa.Shared.Services;

public class LocalApiCallsHttpHandler(IHttpContextAccessor httpContextAccessor, ILogger<LocalApiCallsHttpHandler> logger) : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var cookies = httpContext.Request.Cookies;
                foreach (var cookie in cookies)
                {
                    var cookieValue = Uri.EscapeDataString(cookie.Value);
                    request.Headers.Add("Cookie", new CookieHeaderValue(cookie.Key, cookieValue).ToString());
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex, "Error adding cookies to the request");
        }

        return base.SendAsync(request, cancellationToken);
    }
}
