// Debug helper para autenticación en PWA
(function() {
    'use strict';

    // Verificar que estamos en un entorno donde podemos ejecutar
    if (typeof window === 'undefined') {
        console.warn('AuthDebugger: Window object not available');
        return;
    }

    window.AuthDebugger = {
        // Información del entorno
        getEnvironmentInfo: () => {
            try {
                return {
                    userAgent: navigator.userAgent || 'N/A',
                    platform: navigator.userAgentData?.platform || navigator.platform || 'N/A',
                    displayMode: window.matchMedia('(display-mode: standalone)').matches ? 'standalone' : 'browser',
                    baseUrl: window.location.origin,
                    currentUrl: window.location.href,
                    referrer: document.referrer || 'N/A',
                    isPWA: window.matchMedia('(display-mode: standalone)').matches,
                    hasServiceWorker: 'serviceWorker' in navigator,
                    serviceWorkerState: navigator.serviceWorker?.controller ? 'active' : 'inactive',
                    screenSize: `${screen.width || 0}x${screen.height || 0}`,
                    viewportSize: `${window.innerWidth || 0}x${window.innerHeight || 0}`,
                    online: navigator.onLine,
                    cookieEnabled: navigator.cookieEnabled,
                    timestamp: new Date().toISOString()
                };
            } catch (error) {
                console.warn('AuthDebugger: Error getting environment info:', error);
                return { error: error.message };
            }
        },

        // Logs de autenticación
        logAuthEvent: (event, details) => {
            try {
                const timestamp = new Date().toISOString();
                const info = {
                    timestamp,
                    event: event || 'Unknown Event',
                    details: details || {},
                    environment: window.AuthDebugger.getEnvironmentInfo()
                };
                
                console.group(`🔐 Auth Event: ${event}`);
                console.log('Details:', details);
                console.log('Environment:', info.environment);
                console.groupEnd();
                
                // Guardar en localStorage para debugging
                try {
                    const logs = JSON.parse(localStorage.getItem('auth-debug-logs') || '[]');
                    logs.push(info);
                    // Mantener solo los últimos 50 logs
                    if (logs.length > 50) {
                        logs.splice(0, logs.length - 50);
                    }
                    localStorage.setItem('auth-debug-logs', JSON.stringify(logs));
                } catch (storageError) {
                    console.warn('Could not save auth debug log:', storageError);
                }
            } catch (error) {
                console.warn('AuthDebugger: Error logging event:', error);
            }
        },

        // Obtener logs guardados
        getStoredLogs: () => {
            try {
                return JSON.parse(localStorage.getItem('auth-debug-logs') || '[]');
            } catch (error) {
                console.warn('Could not retrieve auth debug logs:', error);
                return [];
            }
        },

        // Limpiar logs
        clearLogs: () => {
            try {
                localStorage.removeItem('auth-debug-logs');
                console.log('🔐 Auth debug logs cleared');
            } catch (error) {
                console.warn('Could not clear auth debug logs:', error);
            }
        },

        // Test de conectividad
        testConnectivity: async () => {
            const tests = [];
            
            // Test base URL
            try {
                const response = await fetch(window.location.origin, {
                    method: 'HEAD',
                    cache: 'no-cache'
                });
                tests.push({
                    test: 'Base URL',
                    status: response.ok ? 'OK' : 'FAIL',
                    details: `${response.status} ${response.statusText}`
                });
            } catch (error) {
                tests.push({
                    test: 'Base URL',
                    status: 'ERROR',
                    details: error.message
                });
            }

            // Test callback URL specifically
            try {
                const callbackUrl = `${window.location.origin}/authentication/login-callback`;
                const response = await fetch(callbackUrl, { 
                    method: 'HEAD',
                    cache: 'no-cache'
                });
                tests.push({
                    test: 'Auth Callback URL',
                    status: response.ok || response.status === 200 ? 'OK' : 'FAIL',
                    details: `${response.status} ${response.statusText}`
                });
            } catch (error) {
                tests.push({
                    test: 'Auth Callback URL',
                    status: 'ERROR',
                    details: error.message
                });
            }

            // Test staticwebapps.config.json
            try {
                const configUrl = `${window.location.origin}/staticwebapps.config.json`;
                const response = await fetch(configUrl, { 
                    method: 'HEAD',
                    cache: 'no-cache'
                });
                tests.push({
                    test: 'StaticWebApps Config',
                    status: response.ok ? 'OK' : 'FAIL',
                    details: `${response.status} ${response.statusText}`
                });
            } catch (error) {
                tests.push({
                    test: 'StaticWebApps Config',
                    status: 'ERROR',
                    details: error.message
                });
            }

            // Test Auth0 authority si está disponible
            const authConfig = window.authConfig || {};
            if (authConfig.Authority) {
                try {
                    const wellKnownUrl = `${authConfig.Authority}/.well-known/openid_configuration`;
                    const response = await fetch(wellKnownUrl);
                    tests.push({
                        test: 'Auth0 Well-Known',
                        status: response.ok ? 'OK' : 'FAIL',
                        details: `${response.status} ${response.statusText}`
                    });
                } catch (error) {
                    tests.push({
                        test: 'Auth0 Well-Known',
                        status: 'ERROR',
                        details: error.message
                    });
                }
            } else {
                tests.push({
                    test: 'Auth0 Configuration',
                    status: 'WARNING',
                    details: 'Auth configuration not found in window.authConfig'
                });
            }

            return tests;
        },

        // Verificar configuración
        checkConfiguration: () => {
            const config = window.authConfig || {};
            const issues = [];

            if (!config.Authority) issues.push('Missing Authority');
            if (!config.ClientId) issues.push('Missing ClientId');
            if (!config.Audience) issues.push('Missing Audience');

            return {
                config,
                issues,
                isValid: issues.length === 0
            };
        },

        // Test específico para PWA
        testPWARouting: async () => {
            const routes = [
                '/authentication/login-callback',
                '/authentication/logout-callback',
                '/authentication/login-failed',
                '/authentication/logout-failed'
            ];

            const results = [];
            
            for (const route of routes) {
                try {
                    const url = `${window.location.origin}${route}`;
                    const response = await fetch(url, { 
                        method: 'HEAD',
                        cache: 'no-cache'
                    });
                    
                    results.push({
                        route,
                        status: response.status,
                        ok: response.ok || response.status === 200,
                        statusText: response.statusText
                    });
                } catch (error) {
                    results.push({
                        route,
                        status: 'ERROR',
                        ok: false,
                        statusText: error.message
                    });
                }
            }
            
            return results;
        },

        // Simular callback para testing
        simulateCallback: (code, state) => {
            try {
                const callbackUrl = `${window.location.origin}/authentication/login-callback?code=${code || 'test-code'}&state=${state || 'test-state'}`;
                
                window.AuthDebugger.logAuthEvent('Simulated Callback', {
                    url: callbackUrl,
                    code: code?.substring(0, 10) + '...',
                    state,
                    isPWA: window.matchMedia('(display-mode: standalone)').matches
                });

                if (window.matchMedia('(display-mode: standalone)').matches) {
                    // En PWA, usar window.location
                    console.log('🔐 PWA Mode: Using window.location for navigation');
                    window.location.href = callbackUrl;
                } else {
                    // En browser, usar history API
                    console.log('🔐 Browser Mode: Using history API for navigation');
                    window.history.pushState({}, '', callbackUrl);
                    window.dispatchEvent(new PopStateEvent('popstate'));
                }
            } catch (error) {
                console.error('Error simulating callback:', error);
            }
        },

        // Obtener información completa para reporte
        generateReport: async () => {
            try {
                const report = {
                    timestamp: new Date().toISOString(),
                    environment: window.AuthDebugger.getEnvironmentInfo(),
                    configuration: window.AuthDebugger.checkConfiguration(),
                    connectivity: await window.AuthDebugger.testConnectivity(),
                    pwaRouting: await window.AuthDebugger.testPWARouting(),
                    recentLogs: window.AuthDebugger.getStoredLogs().slice(-10),
                    serviceWorkerInfo: await window.AuthDebugger.getServiceWorkerInfo()
                };
                
                console.group('🔐 Complete Auth Debug Report');
                console.log(JSON.stringify(report, null, 2));
                console.groupEnd();
                
                return report;
            } catch (error) {
                console.error('Error generating report:', error);
                return { error: error.message };
            }
        },

        // Información del Service Worker
        getServiceWorkerInfo: async () => {
            if (!('serviceWorker' in navigator)) {
                return { available: false };
            }

            try {
                const registration = await navigator.serviceWorker.getRegistration();
                return {
                    available: true,
                    registered: !!registration,
                    active: !!registration?.active,
                    waiting: !!registration?.waiting,
                    installing: !!registration?.installing,
                    updateViaCache: registration?.updateViaCache,
                    scope: registration?.scope
                };
            } catch (error) {
                return {
                    available: true,
                    error: error.message
                };
            }
        }
    };

    // Interceptar navegación para logging
    try {
        const originalPushState = history.pushState;
        const originalReplaceState = history.replaceState;

        history.pushState = function(state, title, url) {
            if (url && url.includes('authentication')) {
                window.AuthDebugger.logAuthEvent('Navigation (pushState)', { url, state });
            }
            return originalPushState.apply(this, arguments);
        };

        history.replaceState = function(state, title, url) {
            if (url && url.includes('authentication')) {
                window.AuthDebugger.logAuthEvent('Navigation (replaceState)', { url, state });
            }
            return originalReplaceState.apply(this, arguments);
        };
    } catch (error) {
        console.warn('AuthDebugger: Could not intercept navigation:', error);
    }

    // Interceptar window.location changes
    try {
        let currentUrl = window.location.href;
        setInterval(() => {
            if (window.location.href !== currentUrl) {
                if (window.location.href.includes('authentication')) {
                    window.AuthDebugger.logAuthEvent('Location Change', {
                        from: currentUrl,
                        to: window.location.href
                    });
                }
                currentUrl = window.location.href;
            }
        }, 1000);
    } catch (error) {
        console.warn('AuthDebugger: Could not monitor location changes:', error);
    }

    // Interceptar errores de red que puedan ser relacionados con autenticación
    try {
        window.addEventListener('error', (event) => {
            if (event.filename && event.filename.includes('authentication')) {
                window.AuthDebugger.logAuthEvent('Script Error', {
                    message: event.message,
                    filename: event.filename,
                    lineno: event.lineno,
                    colno: event.colno
                });
            }
        });
    } catch (error) {
        console.warn('AuthDebugger: Could not set error listener:', error);
    }

    // Interceptar rechazos de promesas no manejados
    try {
        window.addEventListener('unhandledrejection', (event) => {
            if (event.reason && event.reason.toString().toLowerCase().includes('auth')) {
                window.AuthDebugger.logAuthEvent('Unhandled Promise Rejection', {
                    reason: event.reason.toString(),
                    stack: event.reason.stack
                });
            }
        });
    } catch (error) {
        console.warn('AuthDebugger: Could not set rejection listener:', error);
    }

    console.log('🔐 Auth Debugger loaded successfully');
    console.log('🔧 Available methods: getEnvironmentInfo(), testConnectivity(), testPWARouting(), generateReport()');

    // Log inicial
    window.AuthDebugger.logAuthEvent('AuthDebugger Initialized', {
        timestamp: new Date().toISOString(),
        url: window.location.href,
        isPWA: window.matchMedia('(display-mode: standalone)').matches
    });

})();