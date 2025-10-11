# Azure Application Insights Configuration Guide

This guide explains how to configure Azure Application Insights for the Gastos.Api project using OpenTelemetry.

## Current Configuration

The application is now configured to automatically send logs, metrics, and traces to Azure Application Insights when a connection string is provided. The configuration includes:

1. **OpenTelemetry with Azure Monitor** integration in `GastosApp.ServiceDefaults`
2. **Automatic telemetry collection** for:
   - HTTP requests and responses
   - Database operations (via Npgsql.OpenTelemetry)
   - Runtime metrics
   - Custom logging

## Setup Steps

### 1. Create or Find Your Application Insights Resource

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Application Insights** resources
3. Either use an existing resource or create a new one:
   - **Resource Group**: GastosGrupo (same as your API)
   - **Name**: e.g., `GastosApi-AppInsights`
   - **Region**: Spain Central (same as your API)

### 2. Get the Connection String

1. In your Application Insights resource, go to **Overview**
2. Copy the **Connection String** (not the Instrumentation Key)
3. It should look like: `InstrumentationKey=12345678-abcd-abcd-abcd-123456789012;IngestionEndpoint=https://spaincentral-1.in.applicationinsights.azure.com/;LiveEndpoint=https://spaincentral.livediagnostics.monitor.azure.com/;ApplicationId=12345678-abcd-abcd-abcd-123456789012`

### 3. Configure the Connection String

#### Option A: Azure App Service Environment Variables (Recommended for Production)

1. Go to **Azure Portal** → **App Services** → **jcdcGastosApi**
2. Navigate to **Configuration** → **Application settings**
3. Add a new setting:
   - **Name**: `APPLICATIONINSIGHTS_CONNECTION_STRING`
   - **Value**: Your connection string from step 2
4. Click **Save** and restart the app service

#### Option B: Configuration File (For Development)

Add the connection string to your local `appsettings.Development.json`:

```json
{
  "AzureMonitor": {
    "ConnectionString": "InstrumentationKey=12345678-abcd-abcd-abcd-123456789012;IngestionEndpoint=https://spaincentral-1.in.applicationinsights.azure.com/;LiveEndpoint=https://spaincentral.livediagnostics.monitor.azure.com/;ApplicationId=12345678-abcd-abcd-abcd-123456789012"
  }
}
```

> **⚠️ Security Note**: Never commit connection strings to source control. Use Azure App Service configuration or environment variables for production.

## What Will Be Collected

Once configured, the following telemetry will be automatically sent to Application Insights:

### 📊 Metrics
- **HTTP requests**: Duration, status codes, throughput
- **Database operations**: Query performance, connection pooling
- **.NET Runtime**: GC pressure, memory usage, thread pool

### 🔍 Traces
- **HTTP requests**: Full request/response details
- **Database queries**: SQL statements and performance
- **Custom activities**: Any custom tracing you add
- **Distributed tracing**: Across microservices

### 📝 Logs
- **Application logs**: All ILogger calls
- **ASP.NET Core logs**: Framework logs (filtered to Warning+ in production)
- **Custom logs**: Your application-specific logging

### ⚡ Live Metrics
- **Real-time performance**: CPU, memory, requests/second
- **Live failures**: Real-time error monitoring
- **Live dependencies**: Database and HTTP call performance

## Verifying the Configuration

### 1. Check Startup Logs

When the application starts, you should see log entries indicating OpenTelemetry and Azure Monitor are configured:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7023
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### 2. Test with Sample Requests

1. Make some API calls to your endpoints
2. Go to **Application Insights** → **Live metrics** to see real-time data
3. Check **Application Insights** → **Logs** after a few minutes for stored data

### 3. Query Sample Data

In Application Insights **Logs**, try these queries:

```kusto
// Recent requests
requests
| where timestamp > ago(1h)
| order by timestamp desc

// Recent logs
traces
| where timestamp > ago(1h)
| order by timestamp desc

// Performance counters
performanceCounters
| where timestamp > ago(1h)
| order by timestamp desc

// Dependencies (database calls)
dependencies
| where timestamp > ago(1h)
| order by timestamp desc
```

## Troubleshooting

### No Data Appearing in Application Insights

1. **Check connection string**: Verify it's correctly set in Azure App Service configuration
2. **Check app restart**: Azure App Service needs restart after configuration changes
3. **Check logs**: Look for any error messages in App Service logs
4. **Verify region**: Ensure Application Insights resource is in the same region
5. **Wait time**: It can take 3-5 minutes for data to appear

### Partial Data

1. **Check sampling**: Application Insights might be sampling data in high-volume scenarios
2. **Check filters**: Verify no custom filters are excluding data
3. **Check instrumentation**: Ensure all required packages are installed

### High Volume/Cost Concerns

1. **Configure sampling**: Reduce the volume of telemetry data
2. **Filter sensitive data**: Remove PII or sensitive information
3. **Adjust retention**: Configure how long data is stored

## Advanced Configuration

### Custom Telemetry

You can add custom telemetry in your code:

```csharp
// In your controller or service
private readonly ILogger<YourClass> _logger;
private static readonly ActivitySource ActivitySource = new("Gastos.Api");

public async Task<IResult> YourEndpoint()
{
    using var activity = ActivitySource.StartActivity("Custom Operation");
    activity?.SetTag("custom.property", "value");
    
    _logger.LogInformation("Custom operation started");
    
    // Your business logic here
    
    _logger.LogInformation("Custom operation completed");
    return Results.Ok();
}
```

### Environment-Specific Configuration

The current configuration automatically adapts to different environments:

- **Development**: More verbose logging, includes debug information
- **Production**: Optimized for performance, warnings and above only

## Integration with Existing Services

The configuration is designed to work with your existing services:

- **Auth0**: Authentication flows will be traced
- **PostgreSQL**: Database operations are automatically instrumented
- **DocIntel API**: HTTP calls to external APIs are traced
- **Supabase**: Database connections are monitored

## Cost Optimization

To manage Application Insights costs:

1. **Use free tier initially**: 1GB/month is included free
2. **Monitor usage**: Check Data volume in Application Insights
3. **Configure daily cap**: Set maximum daily ingestion
4. **Adjust sampling**: Reduce data volume if needed
5. **Archive old data**: Configure retention policies

## Next Steps

1. **Set up alerts**: Configure alerts for errors or performance issues
2. **Create dashboards**: Build custom dashboards for monitoring
3. **Set up availability tests**: Monitor API endpoint availability
4. **Configure work items**: Integrate with Azure DevOps for automatic bug creation

## References

- [Azure Monitor OpenTelemetry Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview)
- [Application Insights Overview](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [OpenTelemetry .NET Documentation](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel)