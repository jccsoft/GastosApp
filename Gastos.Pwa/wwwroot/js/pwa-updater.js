window.pwaUpdater = {
    dotNetRef: null,
    registration: null,
    refreshing: false,
    newServiceWorker: null,

    initialize: function (dotNetObjectReference) {
        this.dotNetRef = dotNetObjectReference;
        this.setupUpdateHandlers();
        console.log('PWA Updater initialized');
    },

    setupUpdateHandlers: function () {
        if (!('serviceWorker' in navigator)) {
            console.log('Service Worker not supported');
            return;
        }

        navigator.serviceWorker.ready.then((registration) => {
            this.registration = registration;
            console.log('Service Worker ready, current version:', registration.active?.scriptURL);
            
            // Verificar si ya hay un service worker esperando
            if (registration.waiting) {
                console.log('Service Worker waiting found on initialization');
                this.newServiceWorker = registration.waiting;
                this.showUpdateAvailable();
            }

            // Escuchar nuevas instalaciones
            registration.addEventListener('updatefound', () => {
                console.log('New Service Worker installation detected');
                const newWorker = registration.installing;
                this.newServiceWorker = newWorker;

                if (!newWorker) return;

                newWorker.addEventListener('statechange', () => {
                    console.log('New Service Worker state changed to:', newWorker.state);
                    
                    if (newWorker.state === 'installed') {
                        if (navigator.serviceWorker.controller) {
                            // Nueva versión disponible
                            console.log('New version available and ready to activate');
                            this.showUpdateAvailable();
                        } else {
                            // Primera instalación
                            console.log('Service Worker installed for the first time');
                            this.showUpdateInstalled();
                        }
                    }
                    
                    if (newWorker.state === 'activated') {
                        console.log('New Service Worker activated');
                        this.showUpdateReady();
                    }
                });
            });
        }).catch(error => {
            console.error('Error getting service worker ready:', error);
        });

        // Escuchar cambios en el controlador
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (this.refreshing) return;
            console.log('Service Worker controller changed');
            this.handleControllerChange();
        });

        // Escuchar mensajes del service worker
        navigator.serviceWorker.addEventListener('message', (event) => {
            if (event.data) {
                switch (event.data.type) {
                    case 'SKIP_WAITING':
                        console.log('Service Worker skip waiting message received');
                        break;
                    case 'SW_UPDATED':
                        console.log('Service Worker updated message received');
                        this.onSwUpdated(event.data.version);
                        break;
                }
            }
        });

        // Verificar actualizaciones periódicamente (cada 2 minutos)
        setInterval(() => {
            this.checkForUpdates();
        }, 2 * 60 * 1000);

        // Verificar al recuperar el foco de la ventana
        window.addEventListener('focus', () => {
            setTimeout(() => {
                this.checkForUpdates();
            }, 1000);
        });

        // Verificar cuando la conexión vuelve a estar disponible
        window.addEventListener('online', () => {
            setTimeout(() => {
                this.checkForUpdates();
            }, 2000);
        });
    },

    checkForUpdates: function () {
        if (!this.registration) {
            console.log('No service worker registration available');
            return Promise.resolve(false);
        }

        console.log('Checking for updates...');
        
        return this.registration.update().then(() => {
            console.log('Update check completed');
            return true;
        }).catch((error) => {
            console.error('Error checking for updates:', error);
            return false;
        });
    },

    skipWaiting: function () {
        if (this.newServiceWorker) {
            console.log('Telling new service worker to skip waiting');
            this.newServiceWorker.postMessage({ command: 'skipWaiting' });
        } else if (this.registration && this.registration.waiting) {
            console.log('Telling waiting service worker to skip waiting');
            this.registration.waiting.postMessage({ command: 'skipWaiting' });
        } else {
            console.warn('No service worker available to skip waiting');
            // Como fallback, intentar recargar la página
            setTimeout(() => {
                this.reloadApp();
            }, 1000);
        }
    },

    reloadApp: function () {
        console.log('Reloading application...');
        this.refreshing = true;
        // Usar location.replace para evitar problemas con el historial
        window.location.replace(window.location.href);
    },

    handleControllerChange: function () {
        if (this.refreshing) return;
        console.log('Handling controller change - reloading app');
        this.refreshing = true;
        this.reloadApp();
    },

    onSwUpdated: function (version) {
        console.log('Service Worker updated to version:', version);
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateReady').catch(console.error);
        }
    },

    showUpdateAvailable: function () {
        console.log('Notifying .NET about update available');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateAvailable').catch(console.error);
        }
    },

    showUpdateInstalled: function () {
        console.log('Notifying .NET about update installed');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateInstalled').catch(console.error);
        }
    },

    showUpdateReady: function () {
        console.log('Notifying .NET about update ready');
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('OnUpdateReady').catch(console.error);
        }
    },

    // Función de utilidad para limpiar todos los caches manualmente
    clearAllCaches: function () {
        return caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    console.log('Deleting cache:', cacheName);
                    return caches.delete(cacheName);
                })
            );
        }).then(() => {
            console.log('All caches cleared');
            this.reloadApp();
        });
    },

    // Función de diagnóstico
    getServiceWorkerInfo: function () {
        if (!navigator.serviceWorker) {
            return Promise.resolve({ supported: false });
        }

        return navigator.serviceWorker.ready.then(registration => {
            return {
                supported: true,
                active: !!registration.active,
                waiting: !!registration.waiting,
                installing: !!registration.installing,
                scope: registration.scope,
                updateViaCache: registration.updateViaCache,
                activeScriptURL: registration.active?.scriptURL,
                waitingScriptURL: registration.waiting?.scriptURL
            };
        });
    }
};

// Auto-inicializar el updater
document.addEventListener('DOMContentLoaded', () => {
    console.log('PWA Updater script loaded');
    
    // Exponer funciones globales para debugging
    window.clearPWACache = () => window.pwaUpdater.clearAllCaches();
    window.checkPWAUpdates = () => window.pwaUpdater.checkForUpdates();
    window.getPWAInfo = () => window.pwaUpdater.getServiceWorkerInfo();
});