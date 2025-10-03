@echo off
echo 🔨 Building PWA with proper service worker configuration...

REM Build the project
dotnet publish Gastos.Pwa/Gastos.Pwa.csproj -c Release -o publish

REM Verificar que los archivos críticos existan
echo 📁 Checking critical PWA files...

if exist "publish\wwwroot\service-worker.published.js" (
    echo ✅ service-worker.published.js found
) else (
    echo ❌ service-worker.published.js missing!
    exit /b 1
)

if exist "publish\wwwroot\service-worker-assets.js" (
    echo ✅ service-worker-assets.js found
) else (
    echo ⚠️ service-worker-assets.js missing - this might cause issues
)

if exist "publish\wwwroot\staticwebapp.config.json" (
    echo ✅ staticwebapp.config.json found
) else (
    echo ❌ staticwebapp.config.json missing!
    exit /b 1
)

echo 🎉 Build completed successfully!
echo 📦 Files ready for Azure Static Web Apps deployment
pause