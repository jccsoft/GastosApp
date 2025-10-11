using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Gastos.Api.Shared.Extensions;

public static class ApiEndpointsExtensions
{
    public static IEndpointRouteBuilder MapMyApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapProductEndpoints();
        app.MapStoreEndpoints();
        app.MapReceiptEndpoints();
        app.MapSizingEndpoints();
        app.MapStatEndpoints();
        app.MapDiagnosticsEndpoints(); // Add diagnostics endpoints
        return app;
    }

    public static void MapApiEndpointsForwarder(this WebApplication app)
    {
        string pattern = $"{GastosApiEndpoints.ApiBase}/{{**catch-all}}";

        var options = app.Configuration.GetRequiredSection(GastosApiOptions.ConfigurationSection).Get<GastosApiOptions>();

        app.MapForwarder(pattern, options!.BaseUrl,
            transformBuilder =>
            {
                transformBuilder.AddRequestTransform(async transformContext =>
                {
                    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
                    if (httpContextAccessor.HttpContext is not null)
                    {
                        var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        }
                    }
                });
            });
    }
}
