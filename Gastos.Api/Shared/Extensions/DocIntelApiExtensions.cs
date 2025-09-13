using DocIntel.Api.Sdk;
using Yarp.ReverseProxy.Transforms;

namespace Gastos.Api.Shared.Extensions;

public static class DocIntelApiExtensions
{
    public static void MapDocIntelApiForwarder(this WebApplication app)
    {
        string pattern = $"{DocIntelApiEndpoints.ApiBase}/{{**catch-all}}";

        var options = app.Configuration.GetRequiredSection(DocIntelApiOptions.ConfigurationSection).Get<DocIntelApiOptions>();

        app.MapForwarder(pattern, options!.BaseUrl,
            transformBuilder =>
            {
                transformBuilder.AddRequestTransform(transformContext =>
                {
                    var userId = transformContext.HttpContext.User.GetUserId();
                    transformContext.ProxyRequest.Headers.Add(DocIntelApiHeaderKeys.UserId, userId);

                    transformContext.ProxyRequest.Headers.Add(DocIntelApiHeaderKeys.ApiKey, options.ApiKey);

                    return ValueTask.CompletedTask;
                });
            });
    }
}
