// Verificación de routing para PWA - Se ejecuta al cargar la página
(function() {
    'use strict';

    // Función para verificar si estamos en modo PWA
    function isPWAMode() {
        return window.matchMedia('(display-mode: standalone)').matches;
    }

    // Función para log con prefijo PWA
    function pwaLog(message, data = null) {
        const prefix = isPWAMode() ? '🔵 PWA:' : '🌐 WEB:';
        if (data) {
            console.log(prefix, message, data);
        } else {
            console.log(prefix, message);
        }
    }

    // Verificar routing en startup
    function checkRoutingOnStartup() {
        const currentUrl = window.location.href;
        const pathname = window.location.pathname;
        
        pwaLog('Application starting', {
            url: currentUrl,
            pathname: pathname,
            isPWA: isPWAMode(),
            referrer: document.referrer
        });

        // Verificar si estamos en una ruta de callback
        if (pathname.includes('/authentication/')) {
            pwaLog('Authentication route detected', pathname);
            
            // En PWA, verificar que el routing funcione correctamente
            if (isPWAMode()) {
                pwaLog('PWA mode - Authentication routing active');
                
                // Verificar que Blazor Router esté disponible
                setTimeout(() => {
                    if (typeof Blazor !== 'undefined') {
                        pwaLog('Blazor is available');
                    } else {
                        pwaLog('WARNING: Blazor not available yet', 'Authentication routing may fail');
                    }
                }, 1000);
            }
        }
    }

    // Interceptar window.location changes más agresivamente en PWA
    if (isPWAMode()) {
        const originalLocation = window.location;
        let currentHref = window.location.href;
        
        // Monitorear cambios de location más frecuentemente en PWA
        setInterval(() => {
            if (window.location.href !== currentHref) {
                pwaLog('Location changed', {
                    from: currentHref,
                    to: window.location.href
                });
                currentHref = window.location.href;
                
                // Si es una ruta de autenticación, asegurar que no se cachee
                if (currentHref.includes('/authentication/')) {
                    pwaLog('Authentication route change in PWA mode');
                }
            }
        }, 500); // Más frecuente en PWA
    }

    // Verificar Service Worker si estamos en PWA
    function checkServiceWorkerInPWA() {
        if (isPWAMode() && 'serviceWorker' in navigator) {
            navigator.serviceWorker.ready.then(registration => {
                pwaLog('Service Worker ready in PWA mode', {
                    scope: registration.scope,
                    active: !!registration.active
                });
                
                // Verificar que el SW esté manejando correctamente las rutas de auth
                if (registration.active) {
                    // Test simple de routing
                    fetch('/authentication/login-callback', { 
                        method: 'HEAD',
                        cache: 'no-cache' 
                    })
                    .then(response => {
                        pwaLog('Auth route test', {
                            status: response.status,
                            ok: response.ok
                        });
                    })
                    .catch(error => {
                        pwaLog('Auth route test failed', error.message);
                    });
                }
            });
        }
    }

    // Verificar configuración de Auth
    function checkAuthConfiguration() {
        // Buscar la configuración de Auth0 en scripts de Blazor
        setTimeout(() => {
            try {
                // Intentar obtener la configuración desde elementos del DOM o scripts
                const scripts = document.scripts;
                let foundAuthConfig = false;
                
                for (let script of scripts) {
                    if (script.src.includes('AuthenticationService.js')) {
                        foundAuthConfig = true;
                        pwaLog('Authentication service script found');
                        break;
                    }
                }
                
                if (!foundAuthConfig) {
                    pwaLog('WARNING: Authentication service script not found');
                }
                
            } catch (error) {
                pwaLog('Error checking auth configuration', error.message);
            }
        }, 2000);
    }

    // Test específico para PWA routing
    function testPWAAuthRouting() {
        if (!isPWAMode()) return;
        
        const testRoutes = [
            '/authentication/login-callback',
            '/authentication/logout-callback'
        ];
        
        pwaLog('Testing PWA auth routing...');
        
        testRoutes.forEach(route => {
            fetch(`${window.location.origin}${route}`, {
                method: 'HEAD',
                cache: 'no-cache'
            })
            .then(response => {
                pwaLog(`Route test: ${route}`, {
                    status: response.status,
                    ok: response.ok,
                    statusText: response.statusText
                });
            })
            .catch(error => {
                pwaLog(`Route test failed: ${route}`, error.message);
            });
        });
    }

    // Ejecutar verificaciones al cargar
    document.addEventListener('DOMContentLoaded', () => {
        checkRoutingOnStartup();
        checkServiceWorkerInPWA();
        checkAuthConfiguration();
        
        // Test de routing después de un delay
        setTimeout(testPWAAuthRouting, 3000);
    });

    // Si ya está cargado
    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        checkRoutingOnStartup();
        checkServiceWorkerInPWA();
        setTimeout(checkAuthConfiguration, 1000);
        setTimeout(testPWAAuthRouting, 3000);
    }

    // Exportar funciones para debugging manual
    window.PWARoutingDebug = {
        isPWAMode,
        checkRouting: testPWAAuthRouting,
        checkServiceWorker: checkServiceWorkerInPWA,
        checkAuthConfig: checkAuthConfiguration
    };

    pwaLog('PWA Routing Debug loaded');

})();