using System.Diagnostics;

namespace Gastos.Api.Features.Diagnostics;

public static class DiagnosticsEndpoints
{
    public static IEndpointRouteBuilder MapDiagnosticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup($"/{GastosApiEndpoints.ApiBase}/diagnostics")
            .WithTags("Diagnostics")
            .WithOpenApi();

        group.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
            .WithName("GetHealth")
            .WithSummary("Get application health status");

        group.MapGet("/logs/test", TestApplicationInsightsLogs)
            .WithName("TestApplicationInsightsLogs")
            .WithSummary("Test Application Insights logging");

        group.MapGet("/telemetry/test", TestTelemetryGeneration)
            .WithName("TestTelemetryGeneration")
            .WithSummary("Generate test telemetry data");

        return app;
    }

    private static IResult TestApplicationInsightsLogs(ILogger<Program> logger)
    {
        var testId = Guid.NewGuid().ToString("N")[..8];

        logger.LogInformation("🔍 Application Insights Test - Information Log (ID: {TestId})", testId);
        logger.LogWarning("⚠️ Application Insights Test - Warning Log (ID: {TestId})", testId);
        logger.LogError("❌ Application Insights Test - Error Log (ID: {TestId})", testId);

        // Log with custom properties
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["TestId"] = testId,
            ["TestType"] = "ApplicationInsightsValidation",
            ["Component"] = "DiagnosticsEndpoint"
        }))
        {
            logger.LogInformation("📊 Application Insights Test - Scoped Log with custom properties");
        }

        return Results.Ok(new
        {
            Message = "Test logs sent to Application Insights",
            TestId = testId,
            Timestamp = DateTime.UtcNow,
            Instructions = new[]
            {
                "Check Application Insights -> Logs",
                "Run query: traces | where message contains 'Application Insights Test'",
                $"Look for TestId: {testId}",
                "Logs should appear within 3-5 minutes"
            }
        });
    }

    private static readonly ActivitySource ActivitySource = new("Gastos.Api.Diagnostics");

    private static async Task<IResult> TestTelemetryGeneration(ILogger<Program> logger)
    {
        var testId = Guid.NewGuid().ToString("N")[..8];

        using var activity = ActivitySource.StartActivity("TelemetryTest");
        activity?.SetTag("test.id", testId);
        activity?.SetTag("test.type", "manual");
        activity?.SetTag("component", "diagnostics");

        logger.LogInformation("🚀 Starting telemetry generation test (ID: {TestId})", testId);

        // Simulate some work
        await Task.Delay(100);

        // Add some metrics simulation
        activity?.SetTag("simulated.duration", "100ms");
        activity?.SetTag("simulated.operation", "data_processing");

        // Log different levels
        logger.LogDebug("🔧 Debug: Processing test data (ID: {TestId})", testId);
        logger.LogInformation("✅ Success: Test operation completed (ID: {TestId})", testId);

        // Simulate an exception scenario (but don't throw)
        logger.LogWarning("⚠️ Warning: Simulated warning condition (ID: {TestId})", testId);

        return Results.Ok(new
        {
            Message = "Telemetry test completed",
            TestId = testId,
            Timestamp = DateTime.UtcNow,
            GeneratedData = new
            {
                Traces = "Activity with custom tags",
                Logs = "Multiple log levels with test ID",
                Metrics = "Simulated operation metrics"
            },
            CheckInstructions = new[]
            {
                "Application Insights -> Live Metrics (real-time)",
                "Application Insights -> Transaction search",
                "Application Insights -> Logs -> traces table",
                $"Search for TestId: {testId}"
            }
        });
    }
}