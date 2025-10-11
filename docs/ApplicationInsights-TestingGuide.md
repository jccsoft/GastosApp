# Application Insights Testing and Validation Guide

## Quick Setup Checklist

### ✅ Code Changes Completed

1. **Azure Monitor OpenTelemetry Integration**: ✅ Added to ServiceDefaults
2. **Configuration Structure**: ✅ Added AzureMonitor:ConnectionString settings
3. **Enhanced Logging**: ✅ Added configuration status logging
4. **Test Endpoints**: ✅ Added diagnostics endpoints for testing
5. **Build Validation**: ✅ All code compiles successfully

### 🚀 Next Steps - Azure Configuration

## Step 1: Get Your Application Insights Connection String

1. **Navigate to Azure Portal**:
   - Go to [Azure Portal](https://portal.azure.com)
   - Find or create an Application Insights resource
   - Recommended name: `GastosApi-AppInsights`
   - Region: Spain Central (same as your API)

2. **Copy Connection String**:
   - In Application Insights resource → **Overview**
   - Copy the **Connection String** (not Instrumentation Key)
   - Should look like: `InstrumentationKey=...;IngestionEndpoint=https://spaincentral-1.in.applicationinsights.azure.com/;...`

## Step 2: Configure Azure App Service

1. **Navigate to your API App Service**:
   - Go to **App Services** → **jcdcGastosApi**

2. **Add Environment Variable**:
   - Go to **Configuration** → **Application settings**
   - Click **+ New application setting**
   - **Name**: `APPLICATIONINSIGHTS_CONNECTION_STRING`
   - **Value**: Paste your connection string
   - Click **OK** → **Save**

3. **Restart App Service**:
   - Click **Restart** in the Overview page
   - Wait for restart to complete

## Step 3: Verify Configuration

### Method 1: Check Startup Logs

1. **View App Service Logs**:
   - Go to **App Services** → **jcdcGastosApi** → **Log stream**
   - Look for these startup messages:
   ```
   ✅ Application Insights configured with connection string: InstrumentationKey=...
   ```
   
   If you see this instead, the connection string is missing:
   ```
   ⚠️  Application Insights not configured - no connection string found
   ```

### Method 2: Test Endpoints

1. **Access Test Endpoint**:
   ```
   GET https://jcdcgastosapi.azurewebsites.net/api/diagnostics/logs/test
   ```
   
   Expected response:
   ```json
   {
     "message": "Test logs sent to Application Insights",
     "testId": "abc12345",
     "timestamp": "2025-01-15T12:00:00Z",
     "instructions": [
       "Check Application Insights -> Logs",
       "Run query: traces | where message contains 'Application Insights Test'",
       "Look for TestId: abc12345",
       "Logs should appear within 3-5 minutes"
     ]
   }
   ```

2. **Generate Telemetry Data**:
   ```
   GET https://jcdcgastosapi.azurewebsites.net/api/diagnostics/telemetry/test
   ```

## Step 4: Validate Data in Application Insights

### Real-time Validation (Immediate)

1. **Live Metrics**:
   - Go to **Application Insights** → **Live Metrics**
   - Make API calls and see real-time data
   - You should see:
     - Incoming requests
     - Server metrics (CPU, Memory)
     - Request duration

### Historical Data Validation (3-5 minutes delay)

1. **Check Logs**:
   - Go to **Application Insights** → **Logs**
   - Run this query:
   ```kusto
   traces
   | where message contains "Application Insights Test"
   | order by timestamp desc
   | take 10
   ```

2. **Check Requests**:
   ```kusto
   requests
   | where url contains "diagnostics"
   | order by timestamp desc
   | take 10
   ```

3. **Check Performance**:
   ```kusto
   requests
   | where timestamp > ago(1h)
   | summarize avg(duration), count() by name
   | order by avg_duration desc
   ```

## Step 5: Monitor Regular Application Logs

After confirming the test endpoints work, check your regular application data:

### API Requests
```kusto
requests
| where timestamp > ago(1h)
| where url contains "api"
| order by timestamp desc
```

### Application Logs
```kusto
traces
| where timestamp > ago(1h)
| where message contains "Gastos"
| order by timestamp desc
```

### Database Dependencies
```kusto
dependencies
| where type == "SQL"
| where timestamp > ago(1h)
| order by timestamp desc
```

### Errors and Exceptions
```kusto
exceptions
| where timestamp > ago(1h)
| order by timestamp desc
```

## Troubleshooting Common Issues

### ❌ No Data Appearing

**Symptoms**: Live Metrics shows no data, queries return empty results

**Solutions**:
1. **Verify connection string**:
   - Check Azure App Service configuration
   - Ensure connection string is correct format
   - Restart app service after changes

2. **Check app logs**:
   ```
   App Service → Monitoring → Log stream
   ```
   Look for error messages or configuration warnings

3. **Verify region**:
   - Application Insights and App Service should be in same region
   - Check ingestion endpoint in connection string matches your region

### ⚠️ Partial Data Only

**Symptoms**: Some telemetry appears but not all types

**Solutions**:
1. **Check sampling**:
   - High-volume apps may have sampling enabled
   - Go to Application Insights → Usage and estimated costs

2. **Check filters**:
   - Review any custom logging filters in appsettings.json

### 🔄 Data Delay

**Symptoms**: Live Metrics works but historical queries show no data

**Solutions**:
1. **Wait time**: Data can take 3-5 minutes to appear in Logs
2. **Check query time range**: Ensure your queries cover the right time period
3. **Verify timestamps**: Check if server time is correct

## Advanced Verification

### Custom Dashboard

Create a custom dashboard to monitor your API:

1. **Go to Application Insights** → **Workbooks** → **Empty**
2. **Add queries** for:
   - Request rate and duration
   - Error rate
   - Database performance
   - Custom application logs

### Alerts

Set up alerts for:
1. **High error rate**: > 5% error rate in 5 minutes
2. **Slow responses**: Average response time > 2 seconds
3. **Dependency failures**: Database connection issues

### Live Metrics Filters

Configure Live Metrics to show only your API data:
- Filter by `cloud_RoleName = "jcdcGastosApi"`
- Add custom metrics for your specific operations

## Success Criteria

✅ **Configuration Successful When**:
1. App service starts without connection string warnings
2. Live Metrics shows real-time data during API usage
3. Test endpoints return success responses
4. Historical logs show API requests and custom logs
5. Database dependencies are tracked
6. No errors in app service logs related to Application Insights

## Next Steps After Validation

1. **Set up alerts** for critical errors
2. **Create custom dashboards** for monitoring
3. **Configure availability tests** for API endpoints
4. **Set up work item integration** for automatic issue creation
5. **Implement custom telemetry** for business-specific metrics

## Contact Information

If you encounter issues:
1. Check Azure App Service logs first
2. Verify connection string format and permissions
3. Ensure Application Insights resource is accessible
4. Try recreating the Application Insights resource if needed

Remember: It can take up to 5 minutes for data to appear in Application Insights Logs after the first configuration!