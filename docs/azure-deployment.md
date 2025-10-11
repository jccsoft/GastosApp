# Azure Deployment Guide for Gastos App

This guide will help you deploy the Gastos App (API + PWA) to Azure using CI/CD with GitHub Actions.

## 🏗️ Azure Architecture

- **API**: Azure App Service (jcdcGastosApi.azurewebsites.net)
- **PWA**: Azure Static Web Apps (thankful-desert-0e532df03.1.azurestaticapps.net) 
- **Database**: Supabase PostgreSQL (external)
- **Authentication**: Auth0 (external)
- **Monitoring**: Azure Application Insights

## 📋 Prerequisites

1. Azure CLI installed and authenticated
2. GitHub repository connected
3. Azure subscription with active resource group "GastosGrupo"
4. Existing App Service Plan "SpainPlanGratisF1" in Spain Central region

## 🚀 Step 1: Create Azure Resources

### Option A: Using Azure CLI (Bash)
Run the script `scripts/setup-api-azure.sh`:

```bash
# Make script executable
chmod +x scripts/setup-api-azure.sh

# Run the script
./scripts/setup-api-azure.sh
```

### Option B: Using Azure PowerShell
Run the script `scripts/setup-api-azure.ps1`:

```powershell
# Execute the PowerShell script
.\scripts\setup-api-azure.ps1
```

## 🔧 Step 2: Create Azure Static Web App

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Static Web Apps** → **Create**
3. Configure the following:
   - **Resource Group**: GastosGrupo
   - **Name**: jcdcGastosApp
   - **Plan Type**: Free
   - **Region**: Spain Central (or closest available)
   - **Source**: GitHub
   - **Organization**: Your GitHub username/organization
   - **Repository**: GastosApp
   - **Branch**: main
   - **Build Presets**: Blazor
   - **App location**: `/Gastos.Pwa`
   - **Output location**: `wwwroot`

4. Click **Review + Create** → **Create**

## 📊 Step 3: Setup Application Insights

1. **Create Application Insights Resource**:
   - Go to [Azure Portal](https://portal.azure.com)
   - Navigate to **Application Insights** → **Create**
   - **Resource Group**: GastosGrupo
   - **Name**: GastosApi-AppInsights
   - **Region**: Spain Central
   - **Workspace**: Create new or use existing

2. **Get Connection String**:
   - In Application Insights → **Overview**
   - Copy the **Connection String** (not Instrumentation Key)

3. **Configure in App Service**:
   - Go to **App Services** → **jcdcGastosApi**
   - Navigate to **Configuration** → **Application settings**
   - Add: `APPLICATIONINSIGHTS_CONNECTION_STRING` = [Your connection string]
   - Click **Save** and **Restart** the app service

> **✅ Application Insights Setup**: See detailed guide in [`docs/ApplicationInsights-Setup.md`](ApplicationInsights-Setup.md)

## 🔐 Step 4: Configure GitHub Secrets

In your GitHub repository, go to **Settings** → **Secrets and variables** → **Actions** and add these secrets:

### 1. AZURE_API_PUBLISH_PROFILE
1. Go to Azure Portal → App Services → jcdcGastosApi
2. Click **Get publish profile**
3. Copy the entire XML content
4. Add as secret `AZURE_API_PUBLISH_PROFILE`

### 2. AZURE_STATIC_WEB_APPS_API_TOKEN
1. Go to Azure Portal → Static Web Apps → jcdcGastosApp
2. Go to **Overview** → **Manage deployment token**
3. Copy the deployment token
4. Add as secret `AZURE_STATIC_WEB_APPS_API_TOKEN`

## 🔄 Step 5: Configure Auth0 for Production

1. Go to your [Auth0 Dashboard](https://manage.auth0.com/)
2. Select your application
3. Go to **Settings** → **Allowed Callback URLs** and add:
   ```
   https://jcdcgastosapp.azurestaticapps.net/authentication/login-callback
   ```

4. Go to **Allowed Logout URLs** and add:
   ```
   https://jcdcgastosapp.azurestaticapps.net/authentication/logout-callback,
   https://jcdcgastosapp.azurestaticapps.net/authentication/logout-failed,
   https://jcdcgastosapp.azurestaticapps.net/
   ```

5. Go to **Allowed Web Origins** and add:
   ```
   https://jcdcgastosapp.azurestaticapps.net
   ```

6. Go to **Allowed Origins (CORS)** and add:
   ```
   https://jcdcgastosapp.azurestaticapps.net
   ```

## 🎯 Step 6: Test Deployment

1. Push changes to the `main` branch
2. Check GitHub Actions workflow:
   - Go to your repository → **Actions**
   - Monitor the "Deploy Gastos App to Azure" workflow

3. Verify deployments:
   - **API**: https://jcdcgastosapi.azurewebsites.net/swagger
   - **PWA**: https://jcdcgastosapp.azurestaticapps.net

4. **Test Application Insights**:
   - **Test Endpoint**: https://jcdcgastosapi.azurewebsites.net/api/diagnostics/logs/test
   - **Live Metrics**: Application Insights → Live Metrics
   - **Logs**: Application Insights → Logs (3-5 minute delay)

> **🧪 Testing Guide**: See detailed testing steps in [`docs/ApplicationInsights-TestingGuide.md`](ApplicationInsights-TestingGuide.md)

## 🔍 Troubleshooting

### Common Issues

1. **CORS Errors**:
   - Verify CORS configuration in API
   - Check allowed origins in Auth0

2. **Authentication Issues**:
   - Verify Auth0 URLs match exactly
   - Check Client ID and Domain in PWA configuration

3. **API Connection Issues**:
   - Verify API URL in PWA appsettings.json
   - Check environment variables in Azure App Service

4. **Build Failures**:
   - Check GitHub Actions logs
   - Verify all secrets are correctly configured

5. **Application Insights Issues**:
   - Check connection string configuration
   - Verify App Service restart after configuration
   - Use test endpoints to validate telemetry

### Monitoring

- **API Logs**: Azure Portal → App Services → jcdcGastosApi → Log stream
- **Application Insights**: Azure Portal → Application Insights → Live Metrics / Logs
- **PWA Logs**: Azure Portal → Static Web Apps → jcdcGastosApp → Functions → Monitor
- **GitHub Actions**: Repository → Actions tab

## 📊 Environment Variables Summary

### API (Azure App Service)
| Variable | Value |
|----------|--------|
| `GastosApi__BaseUrl` | `https://jcdcgastosapi.azurewebsites.net` |
| `GastosApi__LockUnauthenticated` | `true` |
| `DocIntelApi__BaseUrl` | `https://jcdcdocintelapi-gabncjexamd3gmfk.spaincentral-01.azurewebsites.net` |
| `DocIntelApi__ApiKey` | `3E)W]1^KS8Sb5(^gf:!g` |
| `ConnectionStrings__Default` | `Host=aws-0-eu-west-3.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.rezyxwpgbpdxohejhmuu;Password=Jcb4u5yWnI6R0iHF` |
| `ConnectionStrings__Supabase` | `Host=aws-0-eu-west-3.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.rezyxwpgbpdxohejhmuu;Password=Jcb4u5yWnI6R0iHF` |
| `Auth0__Domain` | `dev-eyfr0qlg3zkooqow.eu.auth0.com` |
| `Auth0__Audience` | `https://gastos-api` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | `InstrumentationKey=...;IngestionEndpoint=https://spaincentral-1.in.applicationinsights.azure.com/;...` |

### PWA Configuration
The PWA uses client-side configuration files:
- `wwwroot/appsettings.json` (production settings)
- `wwwroot/appsettings.Development.json` (development settings)

## 🔄 Continuous Deployment

Every push to the `main` branch will:
1. Build and test both projects
2. Deploy API to Azure App Service
3. Deploy PWA to Azure Static Web Apps

The workflow includes:
- ✅ Build validation
- ✅ Automated testing  
- ✅ Production deployment
- ✅ Environment-specific CORS configuration

## 📈 Monitoring and Observability

### Application Insights Features

- **Real-time monitoring**: Live Metrics stream
- **Request tracking**: API performance and usage
- **Error monitoring**: Exceptions and failed requests  
- **Database monitoring**: PostgreSQL query performance
- **Custom telemetry**: Business-specific metrics
- **Alerts**: Proactive issue detection

### Useful Queries

```kusto
// Recent API requests
requests
| where timestamp > ago(1h)
| order by timestamp desc

// Application errors
exceptions
| where timestamp > ago(1h)
| order by timestamp desc

// Database performance
dependencies
| where type == "SQL"
| summarize avg(duration) by name
```

## 🎉 Success!

Once deployed successfully, your applications will be available at:
- **API**: https://jcdcgastosapi.azurewebsites.net
- **PWA**: https://jcdcgastosapp.azurestaticapps.net
- **Monitoring**: Application Insights → Live Metrics

The PWA will automatically connect to the API and use Auth0 for authentication. All API operations will be monitored and logged in Application Insights.

## 📚 Related Documentation

- [Application Insights Setup Guide](ApplicationInsights-Setup.md)
- [Application Insights Testing Guide](ApplicationInsights-TestingGuide.md)
- [Azure Monitor OpenTelemetry Documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-overview)