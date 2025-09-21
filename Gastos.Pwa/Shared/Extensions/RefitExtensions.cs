using DocIntel.Api.Sdk;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gastos.Pwa.Shared.Extensions;

public static class RefitExtensions
{
    public static IServiceCollection AddRefitClients(
        this IServiceCollection services,
        string baseUrl)
    {
        services
            .AddRefitClient<IDocIntelApi>(baseUrl)
            .AddRefitClient<IProductApi>(baseUrl)
            .AddRefitClient<IStoreApi>(baseUrl)
            .AddRefitClient<IReceiptApi>(baseUrl)
            .AddRefitClient<ISizingApi>(baseUrl)
            .AddRefitClient<IStatApi>(baseUrl);

        return services;
    }


    private static IServiceCollection AddRefitClient<TInterface>(
        this IServiceCollection services,
        string baseUrl)
        where TInterface : class
    {
        void configClient(HttpClient c)
        {
            c.BaseAddress = new Uri(baseUrl);
            c.Timeout = TimeSpan.FromSeconds(30);
        }

        RefitSettings settings = new() { UrlParameterFormatter = new IsoDateTimeUrlParameterFormatter() };

        var builder = services
            .AddRefitClient<TInterface>(settings)
            .ConfigureHttpClient(configClient);

        builder.AddHttpMessageHandler<BearerTokenHttpHandler>();

        return services;
    }


    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// returns the first error message from a Refit ApiResponse error, supporting ProblemDetails with validation errors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="response"></param>
    /// <param name="defaultErrorMessage"></param>
    /// <returns></returns>
    public static string? GetErrorMessage<T>(this ApiResponse<T> response)
    {
        var messages = response.GetErrorMessages();

        return messages?.FirstOrDefault();
    }

    /// <summary>
    /// Extracts a list of error messages from a Refit ApiResponse error, supporting ProblemDetails with validation errors.
    /// Returns null if no messages are found.
    /// </summary>
    public static List<string>? GetErrorMessages<T>(this ApiResponse<T> response)
    {
        if (response?.Error?.Content is null)
            return null;

        try
        {
            // Parse as JsonElement for flexible property access
            var problemElement = JsonSerializer.Deserialize<JsonElement>(response.Error.Content, _jsonOptions);

            // 1. Check for "errors" property at the root (problem.Errors)
            if (problemElement.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
            {
                var messages = ExtractMessagesFromErrorsElement(errorsElement);
                if (messages.Count > 0)
                    return messages;
            }

            // 2. Check for "errors" in extensions (problem.Extensions["errors"])
            if (problemElement.TryGetProperty("extensions", out var extensionsElement) &&
                extensionsElement.ValueKind == JsonValueKind.Object &&
                extensionsElement.TryGetProperty("errors", out var extErrorsElement) &&
                extErrorsElement.ValueKind == JsonValueKind.Object)
            {
                var messages = ExtractMessagesFromErrorsElement(extErrorsElement);
                if (messages.Count > 0)
                    return messages;
            }

            // 3. Fallback to "detail" or "title"
            if (problemElement.TryGetProperty("detail", out var detailElement) && detailElement.ValueKind == JsonValueKind.String)
            {
                var detail = detailElement.GetString();
                if (!string.IsNullOrWhiteSpace(detail))
                    return [detail];
            }
            if (problemElement.TryGetProperty("title", out var titleElement) && titleElement.ValueKind == JsonValueKind.String)
            {
                var title = titleElement.GetString();
                if (!string.IsNullOrWhiteSpace(title))
                    return [title];
            }
        }
        catch
        {
            // Ignore deserialization errors
        }

        // Fallback to Refit error message
        if (!string.IsNullOrWhiteSpace(response.Error?.Message))
            return [response.Error.Message];

        return null;
    }

    private static List<string> ExtractMessagesFromErrorsElement(JsonElement errorsElement)
    {
        var messages = new List<string>();
        foreach (JsonElement propertyValue in errorsElement.EnumerateObject().Select(p => p.Value))
        {
            if (propertyValue.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in propertyValue.EnumerateArray())
                {
                    var msg = item.GetString();
                    if (!string.IsNullOrWhiteSpace(msg))
                        messages.Add(msg);
                }
            }
        }
        return messages;
    }
}