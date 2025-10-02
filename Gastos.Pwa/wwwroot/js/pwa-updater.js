window.pwaUpdater = {
    dotNetRef: null,
    registration: null,
    refreshing: false,
    newServiceWorker: null,

    initialize: function (dotNetObjectReference) {
        this.dotNetRef = dotNetObjectReference;
        console.log('PWA Updater initializing...');
        
        // Si ya tenemos una registración del index.html, usarla
        if (this.registration) {
            console.log('Using existing registration from index.html');
            this.setupUpdateHandlers();
        } else {
            // Si no, registrar nosotros mismos
            this.registerServiceWorker().then(() => {
                this.setupUpdateHandlers();
            });
        }
    },

    registerServiceWorker: function() {
        if (!('serviceWorker' in navigator)) {
            console.log('Service Worker not supported');
            return Promise.resolve(null);
        }

        // Detectar entorno
        const isDevelopment = window.location.hostname === 'localhost' ||
            window.location.hostname === '127.0.0.1' ||
            window.location.hostname.includes('localhost');

        const swPath = isDevelopment ? '/service-worker.js' : '/service-worker.published.js';
        
        console.log(`Registering service worker: ${swPath}`);

        return navigator.serviceWorker.register(swPath, { 
            updateViaCache: 'none',
            scope: '/' 
        })
        .then(registration => {
            console.log(`✅ Service Worker registered successfully (${swPath}):`, registration.scope);
            this.registration = registration;
            return registration;
        })
        .catch(error => {
            console.error('❌ Service Worker registration failed:', error);
            throw error;
        });
    },

    setupUpdateHandlers: function () {
        if (!this.registration) {
            console.error('No service worker registration available');
            return;
        }

        console.log('Setting up update handlers...');
        console.log('Service Worker ready, current version:', this.registration.active?.scriptURL);
        
        // Verificar si ya hay un service worker esperando
        if (this.registration.waiting) {
            console.log('Service Worker waiting found on initialization');
            this.newServiceWorker = this.registration.waiting;
            this.showUpdateAvailable();
        }

        // Escuchar nuevas instalaciones
        this.registration.addEventListener('updatefound', () => {
            console.log('🔍 New Service Worker installation detected');
            const newWorker = this.registration.installing;
            this.newServiceWorker = newWorker;

            if (!newWorker) {
                console.warn('Installing worker not available');
                return;
            }

            newWorker.addEventListener('statechange', () => {
                console.log(`🔄 New Service Worker state changed to: ${newWorker.state}`);
                
                if (newWorker.state === 'installed') {
                    if (navigator.serviceWorker.controller) {
                        // Nueva versión disponible
                        console.log('✅ New version available and ready to activate');
                        this.showUpdateAvailable();
                    } else {
                        // Primera instalación
                        console.log('✅ Service Worker installed for the first time');
                        this.showUpdateInstalled();
                    }
                }
                
                if (newWorker.state === 'activated') {
                    console.log('✅ New Service Worker activated');
                    this.showUpdateReady();
                }
            });
        });

        // Escuchar cambios en el controlador
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (this.refreshing) return;
            console.log('🔄 Service Worker controller changed');
            this.handleControllerChange();
        });

        // Escuchar mensajes del service worker
        navigator.serviceWorker.addEventListener('message', (event) => {
            if (event.data) {
                switch (event.data.type) {
                    case 'SKIP_WAITING':
                        console.log('📨 Service Worker skip waiting message received');
                        break;
                    case 'SW_UPDATED':
                        console.log('📨 Service Worker updated message received');
                        this.onSwUpdated(event.data.version);
                        break;
                }
            }
        });

        // Verificar actualizaciones periódicamente (cada 2 minutos)
        const updateInterval = setInterval(() => {
            this.checkForUpdates();
        }, 2 * 60 * 1000);

        // Limpiar interval si la página se descarga
        window.addEventListener('beforeunload', () => {
            clearInterval(updateInterval);
        });

        // Verificar al recuperar el foco de la ventana
        window.addEventListener('focus', () => {
            setTimeout(() => {
                console.log('🎯 Window focused, checking for updates...');
                this.checkForUpdates();
            }, 1000);
        });

        // Verificar cuando la conexión vuelve a estar disponible
        window.addEventListener('online', () => {
            setTimeout(() => {
                console.log('🌐 Connection restored, checking for updates...');
                this.checkForUpdates();
            }, 2000);
        });

        // Verificación inicial
        setTimeout(() => {
            this.checkForUpdates();
        }, 5000);

        console.log('✅ Update handlers configured successfully');
    },

    checkForUpdates: function () {
        if (!this.registration) {
            console.log('⚠️ No service worker registration available for update check');
            return Promise.resolve(false);
        }

        console.log('🔍 Checking for service worker updates...');
        
        return this.registration.update().then(() => {
            console.log('✅ Update check completed');
            return true;
        }).catch((error) => {
            console.error('❌ Error checking for updates:', error);
            return false;
        });
    },

    skipWaiting: function () {
        console.log('🚀 Attempting to skip waiting...');
        
        if (this.newServiceWorker) {
            console.log('📨 Telling new service worker to skip waiting');
            this.newServiceWorker.postMessage({ command: 'skipWaiting' });
        } else if (this.registration && this.registration.waiting) {
            console.log('📨 Telling waiting service worker to skip waiting');
            this.registration.waiting.postMessage({ command: 'skipWaiting' });
        } else {
            console.warn('⚠️ No service worker available to skip waiting, reloading as fallback...');
            setTimeout(() => {
                this.reloadApp();
            }, 1000);
        }
    },

    reloadApp: function () {
        console.log('🔄 Reloading application...');
        this.refreshing = true;
        // Usar location.replace para evitar problemas con el historial
        window.location.replace(window.location.href);
    },

    handleControllerChange: function () {
        if (this.refreshing) return;
        console.log('🔄 Handling controller change - reloading app');
        this.refreshing = true;
        this.reloadApp();
    },

    onSwUpdated: function (version) {
        console.log('✅ Service Worker updated to version:', version);
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateReady').catch(console.error);
        }
    },

    showUpdateAvailable: function () {
        console.log('📢 Notifying .NET about update available');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateAvailable').catch(error => {
                console.error('❌ Failed to notify .NET about update available:', error);
            });
        } else {
            console.warn('⚠️ .NET reference not available for update notification');
        }
    },

    showUpdateInstalled: function () {
        console.log('📢 Notifying .NET about update installed');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateInstalled').catch(error => {
                console.error('❌ Failed to notify .NET about update installed:', error);
            });
        }
    },

    showUpdateReady: function () {
        console.log('📢 Notifying .NET about update ready');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateReady').catch(error => {
                console.error('❌ Failed to notify .NET about update ready:', error);
            });
        }
    },

    // Función de utilidad para limpiar todos los caches manualmente
    clearAllCaches: function () {
        console.log('🧹 Clearing all caches...');
        return caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    console.log('🗑️ Deleting cache:', cacheName);
                    return caches.delete(cacheName);
                })
            );
        }).then(() => {
            console.log('✅ All caches cleared, reloading...');
            this.reloadApp();
        }).catch(error => {
            console.error('❌ Error clearing caches:', error);
            this.reloadApp();
        });
    },

    // Función de diagnóstico mejorada
    getServiceWorkerInfo: function () {
        if (!navigator.serviceWorker) {
            return Promise.resolve({ supported: false });
        }

        return navigator.serviceWorker.ready.then(registration => {
            const info = {
                supported: true,
                active: !!registration.active,
                waiting: !!registration.waiting,
                installing: !!registration.installing,
                scope: registration.scope,
                updateViaCache: registration.updateViaCache,
                activeScriptURL: registration.active?.scriptURL,
                waitingScriptURL: registration.waiting?.scriptURL,
                installingScriptURL: registration.installing?.scriptURL,
                hasController: !!navigator.serviceWorker.controller,
                controllerURL: navigator.serviceWorker.controller?.scriptURL
            };
            
            console.table(info);
            return info;
        }).catch(error => {
            console.error('❌ Error getting SW info:', error);
            return { supported: true, error: error.message };
        });
    },

    // Función para forzar una verificación de actualización
    forceUpdateCheck: function() {
        console.log('🔄 Forcing update check...');
        return this.checkForUpdates().then(() => {
            if (this.registration && this.registration.waiting) {
                console.log('✅ Update available after force check');
                this.showUpdateAvailable();
                return true;
            }
            console.log('ℹ️ No updates found after force check');
            return false;
        });
    }
};

// Auto-inicializar el updater cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', () => {
    console.log('📦 PWA Updater script loaded');
    
    // Exponer funciones globales para debugging
    window.clearPWACache = () => window.pwaUpdater.clearAllCaches();
    window.checkPWAUpdates = () => window.pwaUpdater.forceUpdateCheck();
    window.getPWAInfo = () => window.pwaUpdater.getServiceWorkerInfo();
    window.forceSWUpdate = () => window.pwaUpdater.skipWaiting();
    
    console.log('🛠️ PWA Debug functions available: clearPWACache(), checkPWAUpdates(), getPWAInfo(), forceSWUpdate()');
});