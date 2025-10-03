// PWA Update Testing Utilities
// Este archivo proporciona herramientas para testing y debugging del sistema de actualizaciones PWA

window.PWAUpdateTester = {
    // Forzar la detección de una actualización simulada
    simulateUpdate: function() {
        console.log('🧪 Simulating PWA update detection...');
        
        if (window.pwaUpdater && window.pwaUpdater.dotNetRef) {
            console.log('📢 Triggering OnUpdateAvailable notification');
            window.pwaUpdater.showUpdateAvailable();
            return true;
        } else {
            console.error('❌ PWA Updater or .NET reference not available');
            return false;
        }
    },

    // Simular que hay una nueva versión esperando
    simulateWaitingServiceWorker: function() {
        console.log('🧪 Simulating waiting service worker...');
        
        if (window.pwaUpdater && window.pwaUpdater.registration) {
            // Crear un mock de service worker waiting
            const mockWaiting = {
                postMessage: (msg) => console.log('📨 Mock SW received message:', msg),
                state: 'installed',
                scriptURL: '/service-worker.published.js?v=mock'
            };
            
            // Simular que hay un SW esperando
            Object.defineProperty(window.pwaUpdater.registration, 'waiting', {
                value: mockWaiting,
                configurable: true
            });
            
            window.pwaUpdater.newServiceWorker = mockWaiting;
            
            console.log('✅ Mock waiting service worker created');
            window.pwaUpdater.showUpdateAvailable();
            return true;
        } else {
            console.error('❌ PWA Updater or registration not available');
            return false;
        }
    },

    // Verificar archivos del service worker
    async checkServiceWorkerFiles() {
        console.log('📁 Checking Service Worker files availability...');
        
        if (window.pwaUpdater && window.pwaUpdater.checkServiceWorkerFiles) {
            return await window.pwaUpdater.checkServiceWorkerFiles();
        } else {
            console.error('❌ PWA Updater checkServiceWorkerFiles function not available');
            return { error: 'Function not available' };
        }
    },

    // Obtener información detallada del estado del service worker
    async getDetailedSWInfo() {
        console.log('🔍 Gathering detailed Service Worker information...');
        
        if (!('serviceWorker' in navigator)) {
            return { error: 'Service Worker not supported' };
        }

        try {
            const registration = await navigator.serviceWorker.ready;
            const controller = navigator.serviceWorker.controller;
            
            const info = {
                // Environment detection
                environment: {
                    detected: window.pwaUpdater?.isDevelopmentEnvironment() ? 'Development' : 'Production',
                    hostname: window.location.hostname,
                    protocol: window.location.protocol,
                    port: window.location.port,
                    origin: window.location.origin,
                    isPWA: window.matchMedia('(display-mode: standalone)').matches,
                    isOnline: navigator.onLine,
                    devNotificationsEnabled: window.pwaUpdater?.isUpdateNotificationEnabledInDev() || false
                },
                
                // Registration info
                registration: {
                    scope: registration.scope,
                    updateViaCache: registration.updateViaCache
                },
                
                // Active SW
                active: {
                    exists: !!registration.active,
                    scriptURL: registration.active?.scriptURL,
                    state: registration.active?.state
                },
                
                // Waiting SW
                waiting: {
                    exists: !!registration.waiting,
                    scriptURL: registration.waiting?.scriptURL,
                    state: registration.waiting?.state
                },
                
                // Installing SW
                installing: {
                    exists: !!registration.installing,
                    scriptURL: registration.installing?.scriptURL,
                    state: registration.installing?.state
                },
                
                // Controller
                controller: {
                    exists: !!controller,
                    scriptURL: controller?.scriptURL,
                    state: controller?.state
                },
                
                // PWA Updater state
                pwaUpdater: {
                    exists: !!window.pwaUpdater,
                    hasRegistration: !!(window.pwaUpdater?.registration),
                    hasNewServiceWorker: !!(window.pwaUpdater?.newServiceWorker),
                    refreshing: window.pwaUpdater?.refreshing,
                    hasDotNetRef: !!(window.pwaUpdater?.dotNetRef)
                }
            };

            console.groupCollapsed('🔍 Service Worker Detailed Information');
            console.table(info.environment);
            console.table(info.active);
            console.table(info.waiting);
            console.table(info.installing);
            console.table(info.controller);
            console.table(info.pwaUpdater);
            console.groupEnd();
            
            return info;
            
        } catch (error) {
            console.error('❌ Error getting SW info:', error);
            return { error: error.message };
        }
    },

    // Verificar la configuración del cache
    async checkCacheInfo() {
        console.log('🗃️ Checking cache information...');
        
        try {
            const cacheNames = await caches.keys();
            const cacheInfo = {};
            
            for (const cacheName of cacheNames) {
                const cache = await caches.open(cacheName);
                const keys = await cache.keys();
                cacheInfo[cacheName] = {
                    entries: keys.length,
                    urls: keys.slice(0, 5).map(req => req.url) // Primeros 5 URLs
                };
            }
            
            console.log('📦 Cache information:', cacheInfo);
            return cacheInfo;
            
        } catch (error) {
            console.error('❌ Error checking cache:', error);
            return { error: error.message };
        }
    },

    // Test de conectividad y actualizaciones
    async testConnectivityAndUpdates() {
        console.log('🌐 Testing connectivity and update mechanism...');
        
        const results = {
            timestamp: new Date().toISOString(),
            environment: window.pwaUpdater?.isDevelopmentEnvironment() ? 'Development' : 'Production',
            connectivity: {
                online: navigator.onLine,
                serviceWorkerSupported: 'serviceWorker' in navigator
            },
            registration: null,
            updateCheck: null,
            fileCheck: null,
            errors: []
        };

        try {
            // Test 1: Verificar archivos del service worker
            try {
                results.fileCheck = await this.checkServiceWorkerFiles();
                
                // Analizar resultados de archivos
                const missingFiles = [];
                const wrongMimeType = [];
                
                for (const [file, status] of Object.entries(results.fileCheck)) {
                    if (!status.exists) {
                        missingFiles.push(file);
                    } else if (status.contentType && !status.contentType.includes('javascript')) {
                        wrongMimeType.push({file, contentType: status.contentType});
                    }
                }
                
                if (missingFiles.length > 0) {
                    results.errors.push(`Missing files: ${missingFiles.join(', ')}`);
                }
                
                if (wrongMimeType.length > 0) {
                    results.errors.push(`Wrong MIME type: ${wrongMimeType.map(w => `${w.file} (${w.contentType})`).join(', ')}`);
                }
                
            } catch (fileError) {
                results.errors.push(`File check failed: ${fileError.message}`);
            }

            // Test 2: Verificar registro del service worker
            if ('serviceWorker' in navigator) {
                const registration = await navigator.serviceWorker.ready;
                results.registration = {
                    success: true,
                    scope: registration.scope,
                    hasActive: !!registration.active,
                    hasWaiting: !!registration.waiting,
                    hasInstalling: !!registration.installing,
                    activeURL: registration.active?.scriptURL,
                    waitingURL: registration.waiting?.scriptURL
                };

                // Test 3: Intentar verificación de actualización
                try {
                    await registration.update();
                    results.updateCheck = { success: true, message: 'Update check completed' };
                    
                    // Verificar si ahora hay algo esperando
                    if (registration.waiting) {
                        results.updateCheck.waitingFound = true;
                        results.updateCheck.waitingURL = registration.waiting.scriptURL;
                    }
                } catch (updateError) {
                    results.updateCheck = { success: false, error: updateError.message };
                    results.errors.push(`Update check failed: ${updateError.message}`);
                }
            } else {
                results.errors.push('Service Worker not supported');
            }

            // Test 4: Verificar PWA Updater
            if (window.pwaUpdater) {
                results.pwaUpdater = {
                    available: true,
                    hasRegistration: !!window.pwaUpdater.registration,
                    hasDotNetRef: !!window.pwaUpdater.dotNetRef,
                    environment: window.pwaUpdater.isDevelopmentEnvironment() ? 'Development' : 'Production',
                    devNotificationsEnabled: window.pwaUpdater.isUpdateNotificationEnabledInDev()
                };
            } else {
                results.errors.push('PWA Updater not available');
            }

            // Test 5: Verificar si las notificaciones funcionarían
            if (window.pwaUpdater && window.pwaUpdater.dotNetRef) {
                results.notificationTest = { canNotify: true };
            } else {
                results.notificationTest = { 
                    canNotify: false, 
                    reason: 'No .NET reference available' 
                };
            }

        } catch (error) {
            results.errors.push(`General error: ${error.message}`);
        }

        console.groupCollapsed('🧪 Update System Test Results');
        console.log(`Environment: ${results.environment}`);
        console.table(results.connectivity);
        if (results.fileCheck) {
            console.log('📁 File Check Results:');
            console.table(results.fileCheck);
        }
        if (results.registration) console.table(results.registration);
        if (results.updateCheck) console.table(results.updateCheck);
        if (results.pwaUpdater) console.table(results.pwaUpdater);
        if (results.notificationTest) console.table(results.notificationTest);
        if (results.errors.length > 0) {
            console.error('❌ Errors found:', results.errors);
        }
        console.groupEnd();
        
        return results;
    },

    // Test específico para PWA instalada
    async testInstalledPWA() {
        console.log('📱 Testing installed PWA update behavior...');
        
        const isStandalone = window.matchMedia('(display-mode: standalone)').matches;
        const isPWA = window.navigator.standalone || isStandalone;
        
        const results = {
            isPWAInstalled: isPWA,
            displayMode: isStandalone ? 'standalone' : 'browser',
            environment: window.pwaUpdater?.isDevelopmentEnvironment() ? 'Development' : 'Production',
            shouldShowNotifications: false,
            reasons: []
        };

        if (isPWA) {
            results.reasons.push('Running as installed PWA');
            
            if (!window.pwaUpdater?.isDevelopmentEnvironment()) {
                results.shouldShowNotifications = true;
                results.reasons.push('Production environment - notifications enabled');
            } else if (window.pwaUpdater?.isUpdateNotificationEnabledInDev()) {
                results.shouldShowNotifications = true;
                results.reasons.push('Development environment but notifications manually enabled');
            } else {
                results.reasons.push('Development environment - notifications suppressed');
                results.enableCommand = 'Run: enablePWADevNotifications()';
            }
        } else {
            results.reasons.push('Running in browser, not installed PWA');
        }

        console.table(results);
        return results;
    },

    // Diagnóstico completo del problema del MIME type
    async diagnoseMimeTypeIssue() {
        console.log('🔍 Diagnosing MIME type issues...');
        
        const results = {
            timestamp: new Date().toISOString(),
            files: {},
            recommendations: []
        };

        const filesToCheck = [
            '/service-worker.js',
            '/service-worker.published.js',
            '/service-worker-assets.js'
        ];

        for (const file of filesToCheck) {
            try {
                const response = await fetch(file, { method: 'HEAD' });
                results.files[file] = {
                    status: response.status,
                    statusText: response.statusText,
                    contentType: response.headers.get('content-type'),
                    cacheControl: response.headers.get('cache-control'),
                    exists: response.ok
                };

                // Analizar problemas
                if (!response.ok) {
                    results.recommendations.push(`${file}: File not found (${response.status})`);
                } else {
                    const contentType = response.headers.get('content-type');
                    if (!contentType || !contentType.includes('javascript')) {
                        results.recommendations.push(`${file}: Wrong MIME type (${contentType || 'none'}), should be application/javascript`);
                    }
                }

            } catch (error) {
                results.files[file] = {
                    error: error.message,
                    exists: false
                };
                results.recommendations.push(`${file}: Fetch failed - ${error.message}`);
            }
        }

        // Agregar recomendaciones generales
        if (results.recommendations.length > 0) {
            results.recommendations.push('');
            results.recommendations.push('🔧 Solutions:');
            results.recommendations.push('1. Check staticwebapp.config.json for correct MIME types');
            results.recommendations.push('2. Ensure files are deployed correctly');
            results.recommendations.push('3. Verify Azure Static Web Apps configuration');
            results.recommendations.push('4. Try force refresh or clear cache');
        }

        console.groupCollapsed('🔍 MIME Type Diagnosis');
        console.table(results.files);
        console.log('📋 Recommendations:');
        results.recommendations.forEach(rec => console.log(rec));
        console.groupEnd();

        return results;
    },

    // Limpiar todo y reiniciar
    async resetPWA() {
        console.log('🔄 Resetting PWA state...');
        
        try {
            // 1. Limpiar todos los caches
            const cacheNames = await caches.keys();
            await Promise.all(cacheNames.map(name => caches.delete(name)));
            console.log('✅ All caches cleared');

            // 2. Desregistrar service worker
            if ('serviceWorker' in navigator) {
                const registrations = await navigator.serviceWorker.getRegistrations();
                await Promise.all(registrations.map(reg => reg.unregister()));
                console.log('✅ Service workers unregistered');
            }

            // 3. Limpiar configuración de desarrollo
            localStorage.removeItem('pwa-dev-notifications');
            sessionStorage.removeItem('pwa-dev-notifications');

            // 4. Recargar la página
            setTimeout(() => {
                console.log('🔄 Reloading page...');
                window.location.reload();
            }, 1000);

            return { success: true };
            
        } catch (error) {
            console.error('❌ Error resetting PWA:', error);
            return { success: false, error: error.message };
        }
    },

    // Habilitar modo debug verbose
    enableVerboseLogging: function() {
        window.pwaDebugMode = true;
        console.log('✅ Verbose PWA logging enabled');
    },

    // Mostrar guía de troubleshooting específica para MIME type
    showMimeTypeTroubleshooting: function() {
        console.groupCollapsed('🛠️ MIME Type Issue Troubleshooting Guide');
        console.log('The error "unsupported MIME type (\'text/html\')" usually means:');
        console.log('');
        console.log('🔍 Common causes:');
        console.log('1. Service worker file doesn\'t exist and Azure serves 404 page');
        console.log('2. Azure Static Web Apps misconfiguration');
        console.log('3. Wrong file path or deployment issue');
        console.log('4. Incorrect MIME type headers');
        console.log('');
        console.log('🔧 Solutions to try:');
        console.log('1. Check file availability: checkSWFiles()');
        console.log('2. Diagnose MIME issue: diagnoseMimeTypeIssue()');
        console.log('3. Verify deployment and file paths');
        console.log('4. Check staticwebapp.config.json configuration');
        console.log('5. Force rebuild and redeploy');
        console.log('6. Clear all caches: resetPWA()');
        console.groupEnd();
    },

    // Mostrar guía de troubleshooting general
    showTroubleshootingGuide: function() {
        console.groupCollapsed('🛠️ PWA Update Troubleshooting Guide');
        console.log('1. Check environment detection: getPWAState()');
        console.log('2. Test if installed as PWA: testInstalledPWA()');
        console.log('3. Check file availability: checkSWFiles()');
        console.log('4. Diagnose MIME issues: diagnoseMimeTypeIssue()');
        console.log('5. Force update check: checkPWAUpdates()');
        console.log('6. Enable dev notifications: enablePWADevNotifications()');
        console.log('7. Simulate update: testPWAUpdate()');
        console.log('8. Reset everything: resetPWA()');
        console.log('');
        console.log('Common issues:');
        console.log('- Development environment suppresses notifications');
        console.log('- PWA needs to be installed to test properly');
        console.log('- Service worker may be cached by browser');
        console.log('- Azure Static Web Apps may cache service worker files');
        console.log('- Wrong MIME types causing registration failures');
        console.groupEnd();
    }
};

// Exponer funciones globales para testing
window.testPWAUpdate = () => window.PWAUpdateTester.simulateUpdate();
window.simulateWaitingSW = () => window.PWAUpdateTester.simulateWaitingServiceWorker();
window.getPWAState = () => window.PWAUpdateTester.getDetailedSWInfo();
window.checkPWACache = () => window.PWAUpdateTester.checkCacheInfo();
window.testPWAConnectivity = () => window.PWAUpdateTester.testConnectivityAndUpdates();
window.testInstalledPWA = () => window.PWAUpdateTester.testInstalledPWA();
window.resetPWA = () => window.PWAUpdateTester.resetPWA();
window.diagnoseMimeTypeIssue = () => window.PWAUpdateTester.diagnoseMimeTypeIssue();
window.pwaHelp = () => window.PWAUpdateTester.showTroubleshootingGuide();
window.mimeTypeHelp = () => window.PWAUpdateTester.showMimeTypeTroubleshooting();

console.groupCollapsed('🧪 PWA Update Tester loaded. Available commands:');
console.log('   testPWAUpdate() - Simulate update notification');
console.log('   simulateWaitingSW() - Simulate waiting service worker');
console.log('   getPWAState() - Get detailed SW state');
console.log('   checkPWACache() - Check cache information');
console.log('   testPWAConnectivity() - Test connectivity and updates');
console.log('   testInstalledPWA() - Test PWA installation status');
console.log('   diagnoseMimeTypeIssue() - Diagnose MIME type problems');
console.log('   resetPWA() - Reset PWA state completely');
console.log('   pwaHelp() - Show troubleshooting guide');
console.log('   mimeTypeHelp() - Show MIME type specific help');
console.groupEnd();