# Sistema de Detección de Actualizaciones PWA

Este sistema permite que tu aplicación PWA detecte automáticamente nuevas versiones y ofrezca al usuario la opción de actualizarse.

## Componentes Implementados

### 1. PwaUpdateService (C#)
- **Ubicación**: `Gastos.Pwa/Shared/Services/PwaUpdateService.cs`
- **Función**: Servicio que maneja la comunicación con JavaScript para la detección de actualizaciones
- **Eventos**:
  - `UpdateAvailable`: Se dispara cuando hay una nueva versión disponible
  - `UpdateInstalled`: Se dispara cuando se instala una nueva versión
  - `UpdateReady`: Se dispara cuando la nueva versión está lista

### 2. pwa-updater.js (JavaScript)
- **Ubicación**: `Gastos.Pwa/wwwroot/js/pwa-updater.js`
- **Función**: Maneja la detección de actualizaciones del service worker
- **Características**:
  - Verificación automática cada 5 minutos
  - Verificación cuando la ventana recupera el foco
  - Manejo de eventos del service worker
  - Comunicación bidireccional con .NET

### 3. PwaUpdateNotification (Componente)
- **Ubicación**: `Gastos.Pwa/Components/Shared/PwaUpdateNotification.razor`
- **Función**: Muestra notificaciones de actualización al usuario
- **Características**:
  - Banner de notificación no intrusivo
  - Botones para actualizar ahora o más tarde
  - Indicador de progreso durante la actualización

### 4. PWAStatusCard (Componente)
- **Ubicación**: `Gastos.Pwa/Components/Layout/PWAStatusCard.razor`
- **Función**: Panel de control para verificaciones manuales y estado de la PWA
- **Características**:
  - Estadísticas de verificaciones
  - Botón para verificar actualizaciones manualmente
  - Información técnica de la PWA

## Flujo de Funcionamiento

1. **Inicialización**:
   - Al cargar la aplicación, se registra el service worker
   - Se inicializa el `PwaUpdateService`
   - Se configuran los event listeners

2. **Detección de Actualizaciones**:
   - El sistema verifica actualizaciones automáticamente cada 5 minutos
   - También verifica cuando la ventana recupera el foco
   - El usuario puede verificar manualmente usando el botón correspondiente

3. **Notificación al Usuario**:
   - Cuando se detecta una actualización, aparece un banner en la parte superior
   - El usuario puede elegir "Actualizar ahora" o "Más tarde"
   - Se muestran snackbars con información del proceso

4. **Aplicación de Actualización**:
   - Al hacer clic en "Actualizar ahora", se envía comando al service worker
   - El service worker activa la nueva versión
   - La aplicación se recarga automáticamente

## Configuración

### En index.html
```html
<script src="js/pwa-updater.js"></script>
```

### En DependencyInjection.cs
```csharp
builder.Services.AddScoped<PwaUpdateService>();
```

### En MainLayout.razor
```razor
<PwaUpdateNotification />
```

## Personalización

### Cambiar Frecuencia de Verificación
En `pwa-updater.js`, modifica el intervalo:
```javascript
// Cambiar de 5 minutos a otro valor
setInterval(() => {
    this.checkForUpdates();
}, 10 * 60 * 1000); // 10 minutos
```

### Personalizar Notificaciones
En `PwaUpdateNotification.razor`, modifica los textos y estilos de las alertas y snackbars.

### Personalizar Eventos
Puedes suscribirte a los eventos del `PwaUpdateService` desde cualquier componente:
```csharp
@inject PwaUpdateService PwaUpdateService

protected override void OnInitialized()
{
    PwaUpdateService.UpdateAvailable += OnUpdateAvailable;
}

private async Task OnUpdateAvailable()
{
    // Tu lógica personalizada
}
```

## Consideraciones de Seguridad

- Las actualizaciones solo se aplican si el contenido viene del mismo origen
- El service worker valida la integridad de los archivos usando hashes
- No se cachean rutas de autenticación para evitar problemas de seguridad

## Troubleshooting

### La aplicación no detecta actualizaciones
1. Verificar que el service worker esté registrado correctamente
2. Abrir DevTools > Application > Service Workers
3. Verificar que `updateViaCache: 'none'` esté configurado

### Las actualizaciones no se aplican
1. Verificar la consola del navegador para errores
2. Usar el panel PWAStatusCard para verificar el estado
3. Limpiar cache del navegador si es necesario

### Problemas de autenticación después de actualizar
- Las rutas de autenticación no se cachean intencionalmente
- Si hay problemas, limpiar el cache de la aplicación