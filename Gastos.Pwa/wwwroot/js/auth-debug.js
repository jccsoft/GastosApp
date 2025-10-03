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
                    platform: navigator.userAgentData?.platform || 'N/A',
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

        // Función helper para parsear JSON de forma segura
        parseJsonSafely: async (response) => {
            try {
                const text = await response.text();
                
                // Verificar si el contenido parece ser JSON
                if (text.trim().startsWith('{') || text.trim().startsWith('[')) {
                    return JSON.parse(text);
                } else {
                    console.warn('Response is not JSON:', text.substring(0, 100));
                    return null;
                }
            } catch (error) {
                console.warn('Error parsing JSON:', error.message);
                return null;
            }
        },

        // Obtener configuración de Auth0 desde múltiples fuentes posibles
        getAuth0Configuration: async () => {
            const config = {};
            
            try {
                // Intentar obtener desde appsettings.json
                const response = await fetch('/appsettings.json', { cache: 'no-cache' });
                if (response.ok) {
                    const appSettings = await window.AuthDebugger.parseJsonSafely(response);
                    if (appSettings && appSettings.Auth0) {
                        Object.assign(config, appSettings.Auth0);
                    }
                } else {
                    console.warn(`appsettings.json returned ${response.status}: ${response.statusText}`);
                }
            } catch (error) {
                console.warn('Could not load appsettings.json:', error);
            }

            // También buscar en window.authConfig (si existe)
            if (window.authConfig) {
                Object.assign(config, window.authConfig);
            }

            return config;
        },

        // Test de diagnóstico completo de Azure Static Web Apps
        testAzureDeepDiagnosis: async () => {
            console.groupCollapsed('🔧 === DEEP AZURE STATIC WEB APPS DIAGNOSIS ===');
            
            const diagnosis = {};
            
            // 1. Verificar el archivo de configuración exacto
            try {
                const configResponse = await fetch('/staticwebapp.config.json', { cache: 'no-cache' });
                
                if (configResponse.ok) {
                    const configJson = await window.AuthDebugger.parseJsonSafely(configResponse);
                    
                    if (configJson) {
                        diagnosis.configFile = {
                            exists: true,
                            size: JSON.stringify(configJson).length,
                            routes: configJson.routes,
                            navigationFallback: configJson.navigationFallback,
                            responseOverrides: configJson.responseOverrides
                        };
                        
                        console.log('📄 Config file content:', configJson);
                    } else {
                        const responseText = await configResponse.text();
                        diagnosis.configFile = {
                            exists: false,
                            error: 'Invalid JSON format',
                            content: responseText.substring(0, 200)
                        };
                    }
                } else {
                    diagnosis.configFile = {
                        exists: false,
                        error: `HTTP ${configResponse.status}: ${configResponse.statusText}`
                    };
                }
            } catch (error) {
                diagnosis.configFile = { exists: false, error: error.message };
            }
            
            // 2. Test de cada ruta con información completa
            const routes = [
                '/authentication/login-callback',
                '/authentication/logout-callback',
                '/authentication/login-failed', 
                '/authentication/logout-failed'
            ];
            
            diagnosis.routeTests = [];
            
            for (const route of routes) {
                try {
                    // Test GET completo
                    const response = await fetch(route, { 
                        method: 'GET',
                        cache: 'no-cache',
                        headers: {
                            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8'
                        }
                    });
                    
                    const responseText = await response.text();
                    const responseHeaders = {};
                    response.headers.forEach((value, key) => {
                        responseHeaders[key] = value;
                    });
                    
                    const routeResult = {
                        route,
                        status: response.status,
                        statusText: response.statusText,
                        ok: response.ok,
                        url: response.url,
                        redirected: response.redirected,
                        headers: responseHeaders,
                        contentLength: responseText.length,
                        isIndexHtml: responseText.includes('<div id="app">') && responseText.includes('blazor.webassembly.js'),
                        containsBlazor: responseText.includes('blazor'),
                        contentPreview: responseText.substring(0, 200)
                    };
                    
                    diagnosis.routeTests.push(routeResult);
                    
                    console.log(`${response.ok ? '✅' : '❌'} ${route}:`, routeResult);
                    
                } catch (error) {
                    const errorResult = {
                        route,
                        status: 'ERROR',
                        error: error.message
                    };
                    diagnosis.routeTests.push(errorResult);
                    console.error(`❌ ${route} error:`, error);
                }
            }
            
            // 3. Test específico con parámetros como lo haría Auth0
            try {
                const callbackWithParams = '/authentication/login-callback?code=test123&state=teststate';
                const response = await fetch(callbackWithParams, { 
                    method: 'GET',
                    cache: 'no-cache' 
                });
                
                diagnosis.parameterTest = {
                    url: callbackWithParams,
                    status: response.status,
                    ok: response.ok,
                    finalUrl: response.url,
                    redirected: response.redirected
                };
                
                console.log('🔗 Parameter test:', diagnosis.parameterTest);
            } catch (error) {
                diagnosis.parameterTest = { error: error.message };
            }
            
            // 4. Test de navegación fallback específico
            try {
                const randomRoute = '/this-route-should-not-exist-' + Date.now();
                const response = await fetch(randomRoute, { method: 'GET' });
                
                diagnosis.fallbackTest = {
                    testUrl: randomRoute,
                    status: response.status,
                    shouldBe200: response.status === 200,
                    url: response.url
                };
                
                console.log('🔄 Fallback test:', diagnosis.fallbackTest);
            } catch (error) {
                diagnosis.fallbackTest = { error: error.message };
            }
            
            // 5. Análisis de headers específicos de Azure
            try {
                const response = await fetch('/', { method: 'HEAD' });
                const azureHeaders = {};
                response.headers.forEach((value, key) => {
                    if (key.toLowerCase().includes('azure') || 
                        key.toLowerCase().includes('x-') ||
                        key.toLowerCase().includes('server')) {
                        azureHeaders[key] = value;
                    }
                });
                
                diagnosis.azureHeaders = azureHeaders;
                console.log('🌐 Azure headers:', azureHeaders);
            } catch (error) {
                diagnosis.azureHeaders = { error: error.message };
            }
            
            console.groupEnd();
            
            return diagnosis;
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
                
                console.groupCollapsed(`🔐 Auth Event: ${event}`);
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

        // Test de conectividad mejorado
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

            // Test callback URL specifically - CAMBIO A GET PARA VER CONTENIDO REAL
            try {
                const callbackUrl = `${window.location.origin}/authentication/login-callback`;
                const response = await fetch(callbackUrl, { 
                    method: 'GET',
                    cache: 'no-cache'
                });
                
                const responseText = await response.text();
                const isIndexHtml = responseText.includes('<div id="app">');
                
                tests.push({
                    test: 'Auth Callback URL',
                    status: response.ok && isIndexHtml ? 'OK' : 'FAIL',
                    details: `${response.status} ${response.statusText} - ${isIndexHtml ? 'Returns index.html' : 'Wrong content'}`
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
                const configUrl = `${window.location.origin}/staticwebapp.config.json`;
                const response = await fetch(configUrl, { 
                    method: 'GET',
                    cache: 'no-cache'
                });
                
                if (response.ok) {
                    const configContent = await window.AuthDebugger.parseJsonSafely(response);
                    if (configContent) {
                        tests.push({
                            test: 'StaticWebApps Config',
                            status: 'OK',
                            details: `Found ${configContent.routes?.length || 0} routes configured`
                        });
                    } else {
                        tests.push({
                            test: 'StaticWebApps Config',
                            status: 'FAIL',
                            details: 'File exists but contains invalid JSON'
                        });
                    }
                } else {
                    tests.push({
                        test: 'StaticWebApps Config',
                        status: 'FAIL',
                        details: `${response.status} ${response.statusText}`
                    });
                }
            } catch (error) {
                tests.push({
                    test: 'StaticWebApps Config',
                    status: 'ERROR',
                    details: error.message
                });
            }

            return tests;
        },

        // Verificar configuración mejorada
        checkConfiguration: async () => {
            try {
                const config = await window.AuthDebugger.getAuth0Configuration();
                const issues = [];

                if (!config.Authority) issues.push('Missing Authority');
                if (!config.ClientId) issues.push('Missing ClientId');
                if (!config.Audience) issues.push('Missing Audience');

                return {
                    config,
                    issues,
                    isValid: issues.length === 0
                };
            } catch (error) {
                return {
                    config: {},
                    issues: ['Error loading configuration: ' + error.message],
                    isValid: false
                };
            }
        },

        // Test específico para PWA con información detallada
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
                        method: 'GET', // Cambiado a GET
                        cache: 'no-cache'
                    });
                    
                    const responseText = await response.text();
                    const isIndexHtml = responseText.includes('<div id="app">') && responseText.includes('blazor.webassembly.js');
                    
                    results.push({
                        route,
                        status: response.status,
                        ok: response.ok && isIndexHtml, // OK solo si es 200 Y devuelve index.html
                        statusText: response.statusText,
                        headers: {
                            contentType: response.headers.get('content-type'),
                            cacheControl: response.headers.get('cache-control')
                        },
                        isIndexHtml: isIndexHtml,
                        contentLength: responseText.length
                    });
                } catch (error) {
                    results.push({
                        route,
                        status: 'ERROR',
                        ok: false,
                        statusText: error.message,
                        headers: {},
                        isIndexHtml: false,
                        contentLength: 0
                    });
                }
            }
            
            return results;
        },

        // Test de Azure Static Web Apps específico
        testAzureStaticWebApps: async () => {
            const tests = [];
            
            // Test del archivo de configuración
            try {
                const configUrl = `${window.location.origin}/staticwebapp.config.json`;
                const response = await fetch(configUrl);
                
                if (response.ok) {
                    const config = await window.AuthDebugger.parseJsonSafely(response);
                    if (config) {
                        tests.push({
                            test: 'Config File Exists',
                            status: 'OK',
                            details: 'File found and parsed'
                        });
                        
                        // Verificar rutas configuradas
                        const authRoutes = config.routes?.filter(r => r.route.includes('authentication')) || [];
                        tests.push({
                            test: 'Auth Routes Configured',
                            status: authRoutes.length > 0 ? 'OK' : 'FAIL',
                            details: `${authRoutes.length} auth routes found`
                        });
                        
                        // Verificar navigationFallback
                        tests.push({
                            test: 'Navigation Fallback',
                            status: config.navigationFallback ? 'OK' : 'WARNING',
                            details: config.navigationFallback ? 'Configured' : 'Not configured'
                        });
                    } else {
                        tests.push({
                            test: 'Config File Exists',
                            status: 'FAIL',
                            details: 'File exists but contains invalid JSON'
                        });
                    }
                } else {
                    tests.push({
                        test: 'Config File Exists',
                        status: 'FAIL',
                        details: `${response.status} ${response.statusText}`
                    });
                }
            } catch (error) {
                tests.push({
                    test: 'Config File Exists',
                    status: 'ERROR',
                    details: error.message
                });
            }
            
            return tests;
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
                    configuration: await window.AuthDebugger.checkConfiguration(),
                    connectivity: await window.AuthDebugger.testConnectivity(),
                    pwaRouting: await window.AuthDebugger.testPWARouting(),
                    azureStaticWebApps: await window.AuthDebugger.testAzureStaticWebApps(),
                    deepDiagnosis: await window.AuthDebugger.testAzureDeepDiagnosis(),
                    recentLogs: window.AuthDebugger.getStoredLogs().slice(-10),
                    serviceWorkerInfo: await window.AuthDebugger.getServiceWorkerInfo()
                };
                
                console.groupCollapsed('🔐 Complete Auth Debug Report');
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

    //console.groupCollapsed('🔐 Auth Debugger loaded successfully');
    //console.log('🔧 Methods: getEnvironmentInfo(), testConnectivity(), testPWARouting(), generateReport(),testAzureStaticWebApps(), getAuth0Configuration(), testAzureDeepDiagnosis()');
    //console.groupEnd();

})();