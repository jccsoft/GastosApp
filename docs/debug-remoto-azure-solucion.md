# Guía Completa: Resolución de Error de Debug Remoto en Azure App Service

## Problema
Error `0x80aa0003` al intentar hacer debug remoto en Azure App Service "jcdcGastosApi" desde Visual Studio.

## ⚠️ Problema con Scripts PowerShell (Política de Ejecución)

Si obtienes el error "no se puede cargar el archivo porque la ejecución de scripts está deshabilitada", usa una de estas soluciones:

### 🔧 **Solución 1: Configurar Política de Ejecución (Recomendado)**
Abre PowerShell como **Administrador** y ejecuta:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### 🔧 **Solución 2: Ejecutar con Bypass (Una Sola Vez)**
```powershell
PowerShell -ExecutionPolicy Bypass -File .\Scripts\enable-remote-debug.ps1
PowerShell -ExecutionPolicy Bypass -File .\Scripts\monitor-logs.ps1
```

### 🔧 **Solución 3: Usar Archivos .BAT (Más Simple)**
Ejecuta directamente:
```cmd
.\Scripts\enable-remote-debug.bat
.\Scripts\monitor-logs.bat
.\Scripts\diagnose-debug.bat      # ✨ NUEVO: Diagnóstico automático
.\Scripts\check-debug-status.bat  # ✨ NUEVO: Verificar estado
.\Scripts\test-fixes.bat          # ✨ NUEVO: Probar correcciones
```

### 🔧 **Solución 4: Comandos Directos de Azure CLI**
Ver el archivo `Scripts\azure-cli-commands.ps1` para comandos individuales que puedes copiar y pegar.

## 🛠️ Fix para Error de FileStream (SOLUCIONADO)

**Problema**: Error "out-file : Se solicitó a FileStream que abriera un dispositivo que no era un archivo"

**Causa**: Los scripts usaban redirección `>nul` que PowerShell no puede manejar correctamente en ciertos contextos.

**Solución Aplicada**: 
- ✅ Cambiado `>nul 2>nul` por `>nul 2>&1` 
- ✅ Uso de archivos temporales para capturar errores: `>"%TEMP_FILE%" 2>&1`
- ✅ Limpieza automática de archivos temporales
- ✅ Mejor manejo de errores con información detallada

**Para verificar el fix**: Ejecuta `.\Scripts\test-fixes.bat`

## ✅ Soluciones Implementadas

### 1. Scripts de Configuración Automática (CORREGIDOS)
He creado múltiples versiones de los scripts:
- ✅ `Scripts/enable-remote-debug.ps1` (PowerShell)
- ✅ `Scripts/enable-remote-debug.bat` (Batch - **CORREGIDO** sin error FileStream)
- ✅ `Scripts/monitor-logs.ps1` (PowerShell)
- ✅ `Scripts/monitor-logs.bat` (Batch - **CORREGIDO** sin error FileStream)
- ✅ `Scripts/diagnose-debug.bat` (**CORREGIDO** sin error FileStream)
- ✅ `Scripts/check-debug-status.bat` (**CORREGIDO** sin error FileStream)
- ✅ `Scripts/test-fixes.bat` (✨ **NUEVO** - Verificar correcciones)
- ✅ `Scripts/azure-cli-commands.ps1` (Comandos individuales)

### 2. Logging Estructurado Mejorado
He implementado:
- ✅ Middleware de logging detallado (`RequestLoggingMiddleware`)
- ✅ Configuración mejorada de telemetría en `DependencyInjection.cs`
- ✅ Logging estructurado con scopes en `ProductEndpoints.cs`

### 3. Scripts de Monitoreo en Tiempo Real
Los scripts permiten:
- ✅ Monitorear logs en tiempo real
- ✅ Configurar tipos específicos de logs
- ✅ Descargar logs históricos

## 🔧 Pasos para Resolver el Error

### **🚀 Método Recomendado: Verificar Fix + Diagnóstico**

#### Paso 1: Verificar que las Correcciones Funcionan
```cmd
cd O:\Repos\GastosApp
.\Scripts\test-fixes.bat
```

#### Paso 2: Ejecutar Diagnóstico Automático
```cmd
.\Scripts\diagnose-debug.bat
```
Este script identifica automáticamente problemas y proporciona soluciones específicas.

#### Paso 3: Si el Diagnóstico Indica Problemas
```cmd
# Si necesita configuración
.\Scripts\enable-remote-debug.bat

# Si solo necesita verificar estado
.\Scripts\check-debug-status.bat

# Para monitorear logs
.\Scripts\monitor-logs.bat
```

### **Método A: Usar Archivos .BAT (Más Fácil - CORREGIDOS)**

#### Paso 1: Configurar Debug Remoto
```cmd
cd O:\Repos\GastosApp
.\Scripts\enable-remote-debug.bat
```
**NOTA**: El script corregido ya no produce el error de FileStream.

#### Paso 2: Monitorear Logs
```cmd
.\Scripts\monitor-logs.bat
```

### **Método B: Usar PowerShell con Bypass**

#### Paso 1: Configurar Debug Remoto
```powershell
cd O:\Repos\GastosApp
PowerShell -ExecutionPolicy Bypass -File .\Scripts\enable-remote-debug.ps1
```

#### Paso 2: Monitorear Logs
```powershell
PowerShell -ExecutionPolicy Bypass -File .\Scripts\monitor-logs.ps1
```

### **Método C: Comandos Individuales**

Abre PowerShell o Command Prompt y ejecuta estos comandos uno por uno:

```bash
# 1. Verificar autenticación
az account show

# 2. Configurar .NET 9
az webapp config set --resource-group GastosGrupo --name jcdcGastosApi --net-framework-version "v9.0"

# 3. Habilitar debug remoto
az webapp config set --resource-group GastosGrupo --name jcdcGastosApi --remote-debugging-enabled true

# 4. Configurar 64-bit
az webapp config set --resource-group GastosGrupo --name jcdcGastosApi --use-32bit-worker-process false

# 5. Habilitar logging
az webapp log config --resource-group GastosGrupo --name jcdcGastosApi --application-logging true --level information --detailed-error-messages true --failed-request-tracing true --web-server-logging filesystem

# 6. Ver logs en tiempo real
az webapp log tail --resource-group GastosGrupo --name jcdcGastosApi
```

### Paso 4: Configurar Visual Studio
1. **Debug** → **Attach to Process...**
2. **Connection Type**: "Microsoft Azure App Services"
3. **Connection Target**: "jcdcGastosApi"
4. Hacer clic en **Find...** y seleccionar tu suscripción
5. Esperar a que aparezca el proceso `w3wp.exe`
6. Seleccionar el proceso y hacer clic en **Attach**

### Paso 5: Si Sigue Sin Funcionar
```bash
# Reiniciar el App Service
az webapp restart --resource-group GastosGrupo --name jcdcGastosApi

# Verificar estado
az webapp show --resource-group GastosGrupo --name jcdcGastosApi --query '{name:name, state:state}'
```

## 🚀 Alternativas de Debug (Recomendadas)

### 1. Application Insights (Mejor para Producción)
- Ve a **Azure Portal** → **jcdcGastosApi** → **Application Insights**
- Habilitar si no está habilitado
- Usar **Live Metrics** para monitoreo en tiempo real

### 2. Log Streaming en Azure Portal
- Ve a **Azure Portal** → **jcdcGastosApi** → **Log stream**
- Logs en tiempo real sin necesidad de Visual Studio

### 3. Kudu Console (Debug Avanzado)
- URL: https://jcdcgastosapi.scm.azurewebsites.net
- Acceso completo al filesystem y procesos

### 4. Logging Estructurado Implementado
Los endpoints ahora incluyen logging detallado con:
- ✅ Información de usuario
- ✅ Parámetros de entrada
- ✅ Resultados de operaciones
- ✅ Errores detallados
- ✅ Trace IDs para correlación

## 🐛 Troubleshooting

### ⚠️ **Error FileStream (SOLUCIONADO)**

**Error**: "out-file : Se solicitó a FileStream que abriera un dispositivo que no era un archivo"

**Solución**: 
- ✅ Scripts corregidos para usar archivos temporales
- ✅ Redirecciones cambiadas de `>nul` a `>nul 2>&1`
- ✅ Ejecutar `.\Scripts\test-fixes.bat` para verificar

### ⚠️ **Script Sale Después de "Verificando autenticación con Azure..."**

**Causas:**
1. Azure CLI no está autenticado
2. El proceso de login falló
3. Permisos insuficientes

**Soluciones:**
```cmd
# 1. Ejecutar diagnóstico completo
.\Scripts\diagnose-debug.bat

# 2. Login manual en Azure CLI
az login

# 3. Verificar cuenta actual
az account show

# 4. Verificar que tienes acceso al recurso
az webapp show --resource-group GastosGrupo --name jcdcGastosApi
```

### Error 0x80aa0003
**Causas comunes:**
1. **Permisos insuficientes** → Verificar rol en Azure
2. **App Service no configurado para debug** → Ejecutar script
3. **Plataforma incorrecta** → Cambiar a 64-bit
4. **Visual Studio desactualizado** → Actualizar VS 2022

### Debug Remoto No Encuentra Procesos
**Soluciones:**
1. Esperar 30-60 segundos después de conectar
2. Reiniciar el App Service
3. Verificar que la aplicación está ejecutándose
4. Verificar que el debug remoto está habilitado

### Scripts PowerShell No Se Ejecutan
**Soluciones por orden de preferencia:**
1. **Usar archivos .BAT** → `.\Scripts\enable-remote-debug.bat` (CORREGIDOS)
2. **Bypass temporal** → `PowerShell -ExecutionPolicy Bypass -File .\Scripts\enable-remote-debug.ps1`
3. **Cambiar política** → `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser` (como Admin)
4. **Comandos individuales** → Ver `Scripts\azure-cli-commands.ps1`

### Logs No Aparecen
**Verificar:**
```bash
# Estado del logging
az webapp log show --resource-group GastosGrupo --name jcdcGastosApi

# Habilitar logging si está deshabilitado
az webapp log config --resource-group GastosGrupo --name jcdcGastosApi --application-logging true --level information
```

## ⚠️ Limitaciones Importantes

1. **Debug remoto se deshabilita automáticamente después de 48 horas**
2. **Solo funciona con plan de App Service (no Consumption)**
3. **Puede afectar el rendimiento en producción**
4. **Requiere que la aplicación esté ejecutándose**

## 🎯 Recomendación Final

Para **desarrollo**: Usar debug remoto ocasionalmente
Para **producción**: Usar Application Insights + Logging estructurado

El logging estructurado implementado te dará información muy detallada sobre el comportamiento de tu API sin necesidad de debug remoto, siendo más seguro para producción.

## 📞 Comandos de Referencia Rápida

### Scripts BAT (Recomendado - CORREGIDOS - No requiere permisos especiales)
```cmd
.\Scripts\test-fixes.bat           # ✨ Verificar correcciones (NUEVO)
.\Scripts\diagnose-debug.bat       # ✨ Diagnóstico completo (CORREGIDO)
.\Scripts\enable-remote-debug.bat  # Configurar debug remoto (CORREGIDO)
.\Scripts\check-debug-status.bat   # ✨ Solo verificar estado (CORREGIDO)
.\Scripts\monitor-logs.bat         # Ver logs en tiempo real (CORREGIDO)
```

### PowerShell con Bypass
```powershell
PowerShell -ExecutionPolicy Bypass -File .\Scripts\enable-remote-debug.ps1
PowerShell -ExecutionPolicy Bypass -File .\Scripts\monitor-logs.ps1
```

### Comandos Azure CLI Directos
```bash
# Reiniciar App Service
az webapp restart --resource-group GastosGrupo --name jcdcGastosApi

# Descargar logs
az webapp log download --resource-group GastosGrupo --name jcdcGastosApi --log-file logs.zip

# Ver estado actual
az webapp show --resource-group GastosGrupo --name jcdcGastosApi --query '{name:name, state:state, defaultHostName:defaultHostName}'
```

## 🆕 Correcciones Implementadas

### Fix del Error FileStream
- ✅ **Problema**: Redirecciones `>nul` causaban error FileStream
- ✅ **Solución**: Uso de archivos temporales `>"%TEMP_FILE%" 2>&1`
- ✅ **Resultado**: Scripts funcionan sin errores de redirección
- ✅ **Verificación**: Ejecutar `.\Scripts\test-fixes.bat`

### Scripts Corregidos
- ✅ `enable-remote-debug.bat` - Sin errores FileStream
- ✅ `check-debug-status.bat` - Sin errores FileStream
- ✅ `diagnose-debug.bat` - Sin errores FileStream
- ✅ `monitor-logs.bat` - Sin errores FileStream

### Nuevas Características
- ✅ Manejo robusto de archivos temporales
- ✅ Limpieza automática de archivos temporales
- ✅ Mejor información de errores
- ✅ Verificación de correcciones con `test-fixes.bat`