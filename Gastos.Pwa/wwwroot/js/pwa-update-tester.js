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
                // Registration info
                scope: registration.scope,
                updateViaCache: registration.updateViaCache,
                
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
                },
                
                // Environment
                environment: {
                    hostname: window.location.hostname,
                    isDevelopment: window.location.hostname.includes('localhost'),
                    isPWA: window.matchMedia('(display-mode: standalone)').matches,
                    isOnline: navigator.onLine,
                    userAgent: navigator.userAgent
                }
            };

            console.table(info.active);
            console.table(info.waiting);
            console.table(info.installing);
            console.table(info.controller);
            console.table(info.pwaUpdater);
            console.table(info.environment);
            
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
            connectivity: {
                online: navigator.onLine,
                serviceWorkerSupported: 'serviceWorker' in navigator
            },
            registration: null,
            updateCheck: null,
            errors: []
        };

        try {
            // Test 1: Verificar registro del service worker
            if ('serviceWorker' in navigator) {
                const registration = await navigator.serviceWorker.ready;
                results.registration = {
                    success: true,
                    scope: registration.scope,
                    hasActive: !!registration.active,
                    hasWaiting: !!registration.waiting,
                    hasInstalling: !!registration.installing
                };

                // Test 2: Intentar verificación de actualización
                try {
                    await registration.update();
                    results.updateCheck = { success: true, message: 'Update check completed' };
                } catch (updateError) {
                    results.updateCheck = { success: false, error: updateError.message };
                    results.errors.push(`Update check failed: ${updateError.message}`);
                }
            } else {
                results.errors.push('Service Worker not supported');
            }

            // Test 3: Verificar PWA Updater
            if (window.pwaUpdater) {
                results.pwaUpdater = {
                    available: true,
                    hasRegistration: !!window.pwaUpdater.registration,
                    hasDotNetRef: !!window.pwaUpdater.dotNetRef
                };
            } else {
                results.errors.push('PWA Updater not available');
            }

        } catch (error) {
            results.errors.push(`General error: ${error.message}`);
        }

        console.log('🧪 Test results:', results);
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

            // 3. Recargar la página
            setTimeout(() => {
                console.log('🔄 Reloading page...');
                window.location.reload();
            }, 1000);

            return { success: true };
            
        } catch (error) {
            console.error('❌ Error resetting PWA:', error);
            return { success: false, error: error.message };
        }
    }
};

// Exponer funciones globales para testing
window.testPWAUpdate = () => window.PWAUpdateTester.simulateUpdate();
window.getPWAState = () => window.PWAUpdateTester.getDetailedSWInfo();
window.checkPWACache = () => window.PWAUpdateTester.checkCacheInfo();
window.testPWAConnectivity = () => window.PWAUpdateTester.testConnectivityAndUpdates();
window.resetPWA = () => window.PWAUpdateTester.resetPWA();

console.log('🧪 PWA Update Tester loaded. Available commands:');
console.log('   testPWAUpdate() - Simulate update notification');
console.log('   getPWAState() - Get detailed SW state');
console.log('   checkPWACache() - Check cache information');
console.log('   testPWAConnectivity() - Test connectivity and updates');
console.log('   resetPWA() - Reset PWA state completely');