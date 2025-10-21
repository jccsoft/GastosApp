# 🚀 Gastos App - Azure Deployment

Despliegue automático de la aplicación Gastos (API + PWA) en Azure con CI/CD.

## ⚡ Quick Start

### 1. Configurar Azure App Service (API)
```powershell
# Ejecutar el script de configuración
.\scripts\setup-azure-quick.ps1
```

### 2. Crear Azure Static Web App (PWA)
- Ve al [Portal de Azure](https://portal.azure.com)
- Crear **Static Web App** con estos datos:
  - Nombre: `jcdcGastosApp`
  - Grupo de recursos: `GastosGrupo`
  - Plan: Gratis
  - Repositorio: tu repo de GitHub
  - Rama: `main`
  - Ubicación de la app: `/Gastos.Pwa`
  - Ubicación de salida: `wwwroot`

### 3. Configurar Secretos de GitHub
En tu repositorio GitHub → Settings → Secrets:

- `AZURE_API_PUBLISH_PROFILE`: Perfil de publicación del App Service
- `AZURE_STATIC_WEB_APPS_API_TOKEN`: Token de deployment de Static Web Apps

### 4. Actualizar Auth0
Agregar estas URLs en tu aplicación Auth0:

**Callback URLs:**
```
https://jcdcgastosapp.azurestaticapps.net/authentication/login-callback
```

**Logout URLs:**
```
https://jcdcgastosapp.azurestaticapps.net/authentication/logout-callback
https://jcdcgastosapp.azurestaticapps.net/authentication/logout-failed
https://jcdcgastosapp.azurestaticapps.net/
```

**Allowed Origins:**
```
https://jcdcgastosapp.azurestaticapps.net
```

### 5. ¡Desplegar! 
```bash
git push origin main
```

## 📱 URLs Finales

- **API**: https://jcdcgastosapi.azurewebsites.net
- **PWA**: https://jcdcgastosapp.azurestaticapps.net

## 📚 Documentación Completa

Ver [docs/azure-deployment.md](docs/azure-deployment.md) para la guía detallada.

## 🔄 CI/CD Pipeline

El workflow de GitHub Actions (`/.github/workflows/azure-deploy.yml`) se ejecuta automáticamente en cada push a `main`:

1. ✅ Build & Test
2. 🚀 Deploy API → Azure App Service  
3. 🌐 Deploy PWA → Azure Static Web Apps

## 🛠️ Servicios Utilizados

- **Azure App Service** (API .NET 9)
- **Azure Static Web Apps** (Blazor WebAssembly PWA)
- **Supabase PostgreSQL** (Base de datos)
- **Auth0** (Autenticación)
- **GitHub Actions** (CI/CD)

## 🔧 Configuración de Entorno

Las variables de entorno se configuran automáticamente:
- API: Configuración en Azure App Service
- PWA: Archivos `appsettings.json` en `wwwroot/`

CORS está configurado automáticamente entre los servicios.