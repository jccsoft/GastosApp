using System.Diagnostics;
using System.Text;

namespace Gastos.Api.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldLogRequest(context))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log request details
            var requestInfo = await GetRequestInfoAsync(context);
            logger.LogInformation("Incoming Request: {RequestInfo}", requestInfo);

            // Solo interceptar response body para APIs específicas y respuestas pequeñas
            if (ShouldLogResponseBody(context))
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await next(context);

                stopwatch.Stop();

                // Log response details con el contenido
                var responseInfo = await GetResponseInfoAsync(context, responseBody, stopwatch.ElapsedMilliseconds);
                logger.LogInformation("Outgoing Response: {ResponseInfo}", responseInfo);

                // CRÍTICO: Asegurar que el contenido se copia correctamente
                if (responseBody.Length > 0)
                {
                    responseBody.Position = 0;
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                // Para respuestas grandes o binarias, solo loggear metadata
                await next(context);
                stopwatch.Stop();

                var responseInfo = GetBasicResponseInfo(context, stopwatch.ElapsedMilliseconds);
                logger.LogInformation("Outgoing Response (No Body): {ResponseInfo}", responseInfo);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Error processing request {Method} {Path} - Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
        finally
        {
            // CRÍTICO: Siempre restaurar el stream original
            if (context.Response.Body != originalBodyStream)
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    private static bool ShouldLogRequest(HttpContext context)
    {
        var path = context.Request.Path.Value;

        // No loggear health checks ni swagger
        if (path?.StartsWith("/health") == true ||
            path?.StartsWith("/alive") == true ||
            path?.StartsWith("/swagger") == true ||
            path?.StartsWith("/_framework") == true ||
            path?.StartsWith("/openapi") == true)
        {
            return false;
        }

        return true;
    }

    private static bool ShouldLogResponseBody(HttpContext context)
    {
        var path = context.Request.Path.Value;

        // Solo loggear body de APIs gastos y respuestas pequeñas
        if (path?.StartsWith("/gastos-api") != true)
        {
            return false;
        }

        // No loggear responses muy grandes
        if (context.Response.ContentLength > 50000)
        {
            return false;
        }

        // No loggear contenido binario
        var contentType = context.Response.ContentType;
        if (!string.IsNullOrEmpty(contentType) &&
            !contentType.StartsWith("application/json") &&
            !contentType.StartsWith("text/"))
        {
            return false;
        }

        return true;
    }

    private static async Task<object> GetRequestInfoAsync(HttpContext context)
    {
        var request = context.Request;

        var requestInfo = new
        {
            request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            Headers = request.Headers
                .Where(h => !h.Key.StartsWith("Authorization", StringComparison.OrdinalIgnoreCase))
                .Take(10) // Limitar headers para evitar logs muy grandes
                .ToDictionary(h => h.Key, h => h.Value.ToString()),
            request.ContentType,
            request.ContentLength,
            UserAgent = request.Headers.UserAgent.FirstOrDefault(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserId = context.User?.Identity?.IsAuthenticated == true ?
                     context.User.GetUserId() : "Anonymous",
            TraceId = Activity.Current?.TraceId.ToString(),
            Timestamp = DateTime.UtcNow
        };

        // Log request body para POST/PUT pequeños
        if ((request.Method == "POST" || request.Method == "PUT") &&
            request.ContentLength > 0 &&
            request.ContentLength < 5000 && // Reducir límite
            request.ContentType?.StartsWith("application/json") == true)
        {
            try
            {
                request.EnableBuffering();
                var buffer = new byte[request.ContentLength.Value];
                await request.Body.ReadExactlyAsync(buffer);
                request.Body.Position = 0;

                var bodyText = Encoding.UTF8.GetString(buffer);
                bodyText = MaskSensitiveData(bodyText);

                return new
                {
                    requestInfo.Method,
                    requestInfo.Path,
                    requestInfo.QueryString,
                    requestInfo.Headers,
                    requestInfo.ContentType,
                    requestInfo.ContentLength,
                    requestInfo.UserAgent,
                    requestInfo.RemoteIpAddress,
                    requestInfo.UserId,
                    requestInfo.TraceId,
                    requestInfo.Timestamp,
                    Body = bodyText
                };
            }
            catch (Exception ex)
            {
                // Si falla leer el body, continuar sin él
                return new
                {
                    requestInfo.Method,
                    requestInfo.Path,
                    requestInfo.QueryString,
                    requestInfo.Headers,
                    requestInfo.ContentType,
                    requestInfo.ContentLength,
                    requestInfo.UserAgent,
                    requestInfo.RemoteIpAddress,
                    requestInfo.UserId,
                    requestInfo.TraceId,
                    requestInfo.Timestamp,
                    BodyError = ex.Message
                };
            }
        }

        return requestInfo;
    }

    private static async Task<object> GetResponseInfoAsync(HttpContext context, MemoryStream responseBody, long durationMs)
    {
        var response = context.Response;
        string bodyText = "";

        try
        {
            if (responseBody.Length > 0 && responseBody.Length < 10000)
            {
                responseBody.Position = 0;
                bodyText = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Position = 0;
                bodyText = MaskSensitiveData(bodyText);
            }
        }
        catch (Exception ex)
        {
            bodyText = $"Error reading response body: {ex.Message}";
        }

        return new
        {
            response.StatusCode,
            response.ContentType,
            ContentLength = responseBody.Length,
            Headers = response.Headers
                .Take(10) // Limitar headers
                .ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = bodyText,
            DurationMs = durationMs,
            TraceId = Activity.Current?.TraceId.ToString(),
            Timestamp = DateTime.UtcNow
        };
    }

    private static object GetBasicResponseInfo(HttpContext context, long durationMs)
    {
        var response = context.Response;

        return new
        {
            response.StatusCode,
            response.ContentType,
            response.ContentLength,
            DurationMs = durationMs,
            TraceId = Activity.Current?.TraceId.ToString(),
            Timestamp = DateTime.UtcNow
        };
    }

    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        try
        {
            // Mask common sensitive fields (case insensitive)
            var sensitiveFields = new[] { "password", "token", "secret", "key", "authorization" };

            foreach (var field in sensitiveFields)
            {
                var pattern = $@"""({field}[^""]*)""\s*:\s*""([^""]+)""";
                data = System.Text.RegularExpressions.Regex.Replace(data, pattern,
                    $@"""{field}"": ""***MASKED***""",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return data;
        }
        catch
        {
            return "***ERROR_MASKING_DATA***";
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}