# Guía de Diagnóstico PWA

## 🔧 Herramientas de Diagnóstico Disponibles

Con las mejoras implementadas, ahora tienes acceso a un conjunto completo de herramientas de diagnóstico para tu PWA. Estas herramientas están disponibles en la consola del navegador cuando estás en modo desarrollo (localhost).

## 📊 Comandos de Diagnóstico

### `window.diagnoseApp()`
**Descripción:** Proporciona un diagnóstico completo del estado de la PWA.

**Uso:**
```javascript
window.diagnoseApp()
```

**Información proporcionada:**
- ✅ Soporte del navegador (Service Worker, Cache API, Push API, Notifications)
- 📱 Estado PWA (modo standalone, instalación)
- 🔧 Estado del Service Worker (registros, controlador activo, workers en espera)
- 💾 Información de cache (nombres de cache, cantidad de items)
- 🌍 Información del entorno (URL, hostname, estado de conexión)
- 🛠️ Herramientas disponibles

### `window.clearPWACache()`
**Descripción:** Limpia todos los caches de la PWA y recarga la aplicación.

**Uso:**
```javascript
window.clearPWACache()
```

**Acciones que realiza:**
- 🧹 Elimina todos los caches existentes
- 🚫 Desregistra todos los service workers
- 🔄 Recarga la página forzadamente

**⚠️ Advertencia:** Esta acción eliminará toda la funcionalidad offline temporal.

### `window.checkPWAUpdates()`
**Descripción:** Verifica manualmente si hay actualizaciones disponibles.

**Uso:**
```javascript
window.checkPWAUpdates()
```

**Retorna:** `true` si encuentra actualizaciones, `false` en caso contrario.

### `window.getPWAInfo()`
**Descripción:** Obtiene información detallada del estado de la PWA en formato JSON.

**Uso:**
```javascript
const info = await window.getPWAInfo()
console.log(info)
```

**Información retornada:**
```javascript
{
  supported: true,
  registrations: [...],
  controller: {...},
  caches: [...],
  displayMode: "standalone|browser",
  online: true,
  isDevelopment: true
}
```

### `window.forceSWUpdate()`
**Descripción:** Fuerza la activación de un service worker que está esperando.

**Uso:**
```javascript
window.forceSWUpdate()
```

**Cuándo usar:** Cuando hay un service worker esperando y quieres aplicar la actualización inmediatamente.

## 🚨 Solución de Problemas Comunes

### Problema: La PWA no se actualiza
**Síntomas:**
- La aplicación muestra versiones anteriores
- Los cambios no se reflejan después de actualizar

**Diagnóstico:**
1. Ejecutar `window.diagnoseApp()` en consola
2. Buscar "Service Worker is waiting" en la salida
3. Verificar si hay múltiples registros de SW

**Solución:**
```javascript
// Opción 1: Forzar actualización del SW
window.forceSWUpdate()

// Opción 2: Limpiar completamente (última opción)
window.clearPWACache()
```

### Problema: Funciones de diagnóstico no disponibles
**Síntomas:**
- `window.diagnoseApp is not a function`
- Las herramientas no aparecen en consola

**Posibles causas:**
- No estás en modo desarrollo (localhost)
- Error de carga del script `pwa-diagnostics.js`
- Problemas de orden de carga de scripts

**Solución:**
1. Verificar que estás en localhost
2. Recargar la página con Ctrl+F5
3. Verificar errores en la consola del navegador

### Problema: Service Worker en estado "waiting"
**Síntomas:**
- La aplicación no aplica actualizaciones automáticamente
- Mensaje "Service Worker is waiting" en diagnóstico

**Solución:**
```javascript
window.forceSWUpdate()
```

## 📋 Lista de Verificación para Troubleshooting

Cuando tengas problemas con la PWA, sigue estos pasos en orden:

1. **✅ Verificar herramientas disponibles**
   ```javascript
   // Debe mostrar lista de herramientas disponibles
   console.log(typeof window.diagnoseApp) // "function"
   ```

2. **✅ Ejecutar diagnóstico completo**
   ```javascript
   window.diagnoseApp()
   ```

3. **✅ Verificar estado del Service Worker**
   - Buscar mensajes de "waiting" o "installing"
   - Verificar que el scope sea correcto

4. **✅ Verificar actualizaciones disponibles**
   ```javascript
   window.checkPWAUpdates()
   ```

5. **✅ Si hay SW en espera, forzar activación**
   ```javascript
   window.forceSWUpdate()
   ```

6. **✅ Como último recurso, limpiar cache**
   ```javascript
   window.clearPWACache()
   ```

## 🔍 Interpretación de Logs

### Logs Normales (Todo funcionando bien):
```
✅ Service worker registration successful
📦 Caching X assets...
✅ Service worker: Install complete
🎉 Service worker: Activate - Version: X
✅ Service worker: Activated and claimed all clients
```

### Logs de Actualización:
```
🔄 Service worker update found
📊 Service worker state changed to: installed
✅ New service worker installed, waiting for activation
🚀 Service Worker: skipWaiting command received
🔄 Service worker controller changed - page will reload
```

### Logs de Problema:
```
❌ Service worker registration failed
⚠️ Service Worker is waiting - app might be showing old content
⚠️ Update check failed
```

## 🎯 Mejores Prácticas

1. **Siempre diagnosticar primero:** Antes de tomar acciones drásticas, ejecuta `window.diagnoseApp()`

2. **Usar fuerza mínima:** Intenta `window.forceSWUpdate()` antes que `window.clearPWACache()`

3. **Verificar modo desarrollo:** Las herramientas solo funcionan en localhost

4. **Revisar logs:** La consola del navegador tiene información detallada sobre qué está pasando

5. **Probar en incógnito:** Si hay problemas persistentes, prueba en una ventana incógnito para descartar cache del navegador

---

**Nota:** Estas herramientas están diseñadas para desarrollo y troubleshooting. En producción, el sistema de actualización debe funcionar automáticamente a través de la interfaz de usuario.