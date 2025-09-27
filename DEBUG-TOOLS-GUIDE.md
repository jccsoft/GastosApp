# 🔧 GUÍA DE USO DE HERRAMIENTAS DE DEBUG EN PRODUCCIÓN

## ✅ CAMBIOS APLICADOS

### 1. **auth-debug.js ahora se carga SIEMPRE**
- ✅ Removida la restricción de solo desarrollo
- ✅ Mejores verificaciones de errores
- ✅ Wrapped en IIFE para evitar conflictos
- ✅ Test adicional de staticwebapps.config.json

### 2. **Mejoras en index.html**
- ✅ Script se carga directamente (no condicionalmente)
- ✅ Verificación automática de carga
- ✅ Logging mejorado de environment

### 3. **Estilos CSS para Debug**
- ✅ Estilos específicos para herramientas de debug
- ✅ Indicadores visuales de estado PWA
- ✅ Responsive design para debug

## 🚀 CÓMO USAR LAS HERRAMIENTAS DE DEBUG

### Paso 1: Verificar que AuthDebugger esté disponible

**En la PWA instalada**, abrir DevTools (F12) y en Console ejecutar:

```javascript
// Verificar disponibilidad
typeof window.AuthDebugger
// Debe devolver: "object"
```

### Paso 2: Generar reporte completo

```javascript
// Ejecutar en Console de la PWA instalada
window.AuthDebugger.generateReport().then(report => {
    console.log('📊 REPORTE COMPLETO:', JSON.stringify(report, null, 2));
});
```

### Paso 3: Tests específicos

```javascript
// Test de rutas PWA
window.AuthDebugger.testPWARouting().then(results => {
    console.table(results);
});

// Test de conectividad
window.AuthDebugger.testConnectivity().then(results => {
    console.table(results);
});

// Información del entorno
console.log('🌐 Environment:', window.AuthDebugger.getEnvironmentInfo());
```

### Paso 4: Verificar routing específico

```javascript
// Test manual del callback
fetch('/authentication/login-callback', {
    method: 'HEAD', 
    cache: 'no-cache'
}).then(r => console.log('Callback test:', r.status, r.statusText));

// Test de staticwebapps.config.json
fetch('/staticwebapps.config.json', {
    method: 'HEAD'
}).then(r => console.log('Config test:', r.status, r.statusText));
```

## 📊 USANDO LA PÁGINA DE DEBUG

### Opción Visual: `/debug-auth`

1. **Navegar** a `https://tu-pwa/debug-auth` en la PWA instalada
2. **Hacer clic** en "Test Rutas PWA"  
3. **Revisar** los resultados en la tabla
4. **Generar** reporte completo con el botón correspondiente

### Lo que debes ver:

- ✅ **Estado del Usuario**: Autenticado/No autenticado
- ✅ **Información del Entorno**: PWA/Browser, URLs, etc.
- ✅ **Test Results**: Tabla con estado OK/FAIL para cada ruta
- ✅ **Logs Recientes**: Timeline de eventos de autenticación

## 🔍 INTERPRETACIÓN DE RESULTADOS

### Tests que DEBEN estar en OK:

1. **Base URL**: 200 OK
2. **Auth Callback URL**: 200 OK (crítico)
3. **StaticWebApps Config**: 200 OK (crítico)
4. **Auth0 Well-Known**: 200 OK

### Si Auth Callback URL está FAIL:

```javascript
// Verificar manualmente
fetch('https://tu-dominio/authentication/login-callback', {method: 'HEAD'})
    .then(r => console.log('Status:', r.status, 'Headers:', [...r.headers.entries()]))
```

## 🐛 DEBUGGING ESPECÍFICO DEL ERROR 404

### Comando de Debug Completo:

```javascript
(async () => {
    console.log('🔧 === DEBUG COMPLETO PARA ERROR 404 ===');
    
    // 1. Environment info
    const env = window.AuthDebugger.getEnvironmentInfo();
    console.log('🌐 Environment:', env);
    
    // 2. Test routing
    const routing = await window.AuthDebugger.testPWARouting();
    console.log('🔄 Routing Tests:');
    console.table(routing);
    
    // 3. Test connectivity
    const connectivity = await window.AuthDebugger.testConnectivity();
    console.log('📡 Connectivity Tests:');
    console.table(connectivity);
    
    // 4. Service Worker info
    const swInfo = await window.AuthDebugger.getServiceWorkerInfo();
    console.log('⚙️ Service Worker:', swInfo);
    
    // 5. Recent logs
    const logs = window.AuthDebugger.getStoredLogs().slice(-5);
    console.log('📝 Recent Logs:', logs);
    
    console.log('🔧 === FIN DEBUG COMPLETO ===');
})();
```

### Checks Críticos:

```javascript
// Check 1: staticwebapps.config.json
fetch('/staticwebapps.config.json')
    .then(r => r.json())
    .then(config => console.log('✅ Config loaded:', config))
    .catch(e => console.error('❌ Config not found:', e));

// Check 2: Service Worker state
navigator.serviceWorker.getRegistration()
    .then(reg => console.log('✅ SW Registration:', reg))
    .catch(e => console.error('❌ SW Error:', e));

// Check 3: Auth routes manual test
['/authentication/login-callback', '/authentication/logout-callback'].forEach(route => {
    fetch(route, {method: 'HEAD', cache: 'no-cache'})
        .then(r => console.log(`✅ ${route}:`, r.status))
        .catch(e => console.error(`❌ ${route}:`, e.message));
});
```

## 📞 INFORMACIÓN PARA REPORTE DE PROBLEMAS

Si el problema persiste, ejecutar este comando y enviar el resultado:

```javascript
window.AuthDebugger.generateReport().then(report => {
    // Copiar resultado completo
    const reportText = JSON.stringify(report, null, 2);
    
    // Intentar copiar al portapapeles
    if (navigator.clipboard) {
        navigator.clipboard.writeText(reportText);
        console.log('📋 Report copied to clipboard');
    }
    
    console.log('📊 REPORT FOR SUPPORT:', reportText);
    return report;
});
```

## 🎯 EXPECTATIVAS DESPUÉS DEL DEPLOY

Una vez deployados estos cambios:

1. **auth-debug.js** debe aparecer en DevTools → Sources
2. **window.AuthDebugger** debe estar disponible en Console
3. **Página `/debug-auth`** debe funcionar correctamente
4. **Tests de routing** deben mostrar todas las rutas en OK

Si auth-debug.js sigue sin aparecer, verificar:
- ✅ El archivo está en `wwwroot/js/auth-debug.js`
- ✅ El deploy incluye todos los archivos
- ✅ No hay errores 404 en Network tab para este archivo