// Debug helper para autenticación en PWA
window.AuthDebugger = {
    // Información del entorno
    getEnvironmentInfo: () => {
        return {
            userAgent: navigator.userAgent,
            platform: navigator.platform,
            displayMode: window.matchMedia('(display-mode: standalone)').matches ? 'standalone' : 'browser',
            baseUrl: window.location.origin,
            currentUrl: window.location.href,
            referrer: document.referrer,
            isPWA: window.matchMedia('(display-mode: standalone)').matches,
            hasServiceWorker: 'serviceWorker' in navigator,
            serviceWorkerState: navigator.serviceWorker?.controller ? 'active' : 'inactive'
        };
    },

    // Logs de autenticación
    logAuthEvent: (event, details) => {
        const timestamp = new Date().toISOString();
        const info = {
            timestamp,
            event,
            details,
            environment: window.AuthDebugger.getEnvironmentInfo()
        };
        
        console.group(`🔐 Auth Event: ${event}`);
        console.log('Details:', details);
        console.log('Environment:', info.environment);
        console.groupEnd();
        
        // Guardar en localStorage para debugging
        const logs = JSON.parse(localStorage.getItem('auth-debug-logs') || '[]');
        logs.push(info);
        // Mantener solo los últimos 50 logs
        if (logs.length > 50) {
            logs.splice(0, logs.length - 50);
        }
        localStorage.setItem('auth-debug-logs', JSON.stringify(logs));
    },

    // Obtener logs guardados
    getStoredLogs: () => {
        return JSON.parse(localStorage.getItem('auth-debug-logs') || '[]');
    },

    // Limpiar logs
    clearLogs: () => {
        localStorage.removeItem('auth-debug-logs');
        console.log('🔐 Auth debug logs cleared');
    },

    // Test de conectividad
    testConnectivity: async () => {
        const tests = [];
        
        // Test base URL
        try {
            const response = await fetch(window.location.origin);
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

        // Test Auth0 authority
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

    // Simular callback para testing
    simulateCallback: (code, state) => {
        const callbackUrl = `${window.location.origin}/authentication/login-callback?code=${code}&state=${state}`;
        
        window.AuthDebugger.logAuthEvent('Simulated Callback', {
            url: callbackUrl,
            code: code?.substring(0, 10) + '...',
            state
        });

        if (window.matchMedia('(display-mode: standalone)').matches) {
            // En PWA, usar window.location
            window.location.href = callbackUrl;
        } else {
            // En browser, usar history API
            window.history.pushState({}, '', callbackUrl);
            window.dispatchEvent(new PopStateEvent('popstate'));
        }
    }
};

// Interceptar navegación para logging
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

// Interceptar window.location changes
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

console.log('🔐 Auth Debugger loaded. Use window.AuthDebugger for debugging tools.');