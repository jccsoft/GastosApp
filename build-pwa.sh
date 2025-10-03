#!/bin/bash

echo "🔨 Building PWA with proper service worker configuration..."

# Build the project
dotnet publish Gastos.Pwa/Gastos.Pwa.csproj -c Release -o publish

# Verificar que los archivos críticos existan
echo "📁 Checking critical PWA files..."

if [ -f "publish/wwwroot/service-worker.published.js" ]; then
    echo "✅ service-worker.published.js found"
else
    echo "❌ service-worker.published.js missing!"
    exit 1
fi

if [ -f "publish/wwwroot/service-worker-assets.js" ]; then
    echo "✅ service-worker-assets.js found"
else
    echo "⚠️ service-worker-assets.js missing - this might cause issues"
fi

if [ -f "publish/wwwroot/staticwebapp.config.json" ]; then
    echo "✅ staticwebapp.config.json found"
else
    echo "❌ staticwebapp.config.json missing!"
    exit 1
fi

echo "🎉 Build completed successfully!"
echo "📦 Files ready for Azure Static Web Apps deployment"