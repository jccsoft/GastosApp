using System.Diagnostics;
using System.Text;

namespace Gastos.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldLogRequest(context))
        {
            await LogRequestAsync(context);
        }
        else
        {
            await _next(context);
        }
    }

    private static bool ShouldLogRequest(HttpContext context)
    {
        var path = context.Request.Path.Value;
        
        // No loggear health checks ni swagger
        if (path?.StartsWith("/health") == true ||
            path?.StartsWith("/alive") == true ||
            path?.StartsWith("/swagger") == true ||
            path?.StartsWith("/_framework") == true)
        {
            return false;
        }

        return true;
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log request details
            var requestInfo = await GetRequestInfoAsync(context);
            _logger.LogInformation("Incoming Request: {RequestInfo}", requestInfo);

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();

            // Log response details
            var responseInfo = await GetResponseInfoAsync(context, stopwatch.ElapsedMilliseconds);
            _logger.LogInformation("Outgoing Response: {ResponseInfo}", responseInfo);

            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error processing request {Method} {Path} - Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
            
            // Re-throw para mantener el comportamiento original
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static async Task<object> GetRequestInfoAsync(HttpContext context)
    {
        var request = context.Request;
        
        var requestInfo = new
        {
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            Headers = request.Headers.Where(h => !h.Key.StartsWith("Authorization", StringComparison.OrdinalIgnoreCase))
                                   .ToDictionary(h => h.Key, h => h.Value.ToString()),
            ContentType = request.ContentType,
            ContentLength = request.ContentLength,
            UserAgent = request.Headers.UserAgent.ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            UserId = context.User?.Identity?.IsAuthenticated == true ? 
                     context.User.GetUserId() : "Anonymous",
            TraceId = Activity.Current?.TraceId.ToString(),
            SpanId = Activity.Current?.SpanId.ToString(),
            Timestamp = DateTime.UtcNow
        };

        // Log request body for POST/PUT (excluding sensitive data)
        if ((request.Method == "POST" || request.Method == "PUT") && 
            request.ContentLength > 0 && 
            request.ContentLength < 10000) // Limit body size
        {
            request.EnableBuffering();
            var buffer = new byte[request.ContentLength.Value];
            await request.Body.ReadExactlyAsync(buffer);
            request.Body.Position = 0;
            
            var bodyText = Encoding.UTF8.GetString(buffer);
            
            // Mask sensitive fields
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
                requestInfo.SpanId,
                requestInfo.Timestamp,
                Body = bodyText
            };
        }

        return requestInfo;
    }

    private static async Task<object> GetResponseInfoAsync(HttpContext context, long durationMs)
    {
        var response = context.Response;
        
        var responseBody = "";
        if (response.Body is MemoryStream memoryStream && 
            memoryStream.Length > 0 && 
            memoryStream.Length < 10000)
        {
            memoryStream.Position = 0;
            responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            memoryStream.Position = 0;
            
            responseBody = MaskSensitiveData(responseBody);
        }

        return new
        {
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            ContentLength = response.ContentLength,
            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = responseBody,
            DurationMs = durationMs,
            TraceId = Activity.Current?.TraceId.ToString(),
            SpanId = Activity.Current?.SpanId.ToString(),
            Timestamp = DateTime.UtcNow
        };
    }

    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

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
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}