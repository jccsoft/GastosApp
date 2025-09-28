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
            
            // Verificar si ya hay un service worker esperando
            if (registration.waiting) {
                console.log('Service Worker waiting found');
                this.showUpdateAvailable();
            }

            // Escuchar nuevas instalaciones
            registration.addEventListener('updatefound', () => {
                console.log('New Service Worker found');
                const newWorker = registration.installing;
                this.newServiceWorker = newWorker;

                newWorker.addEventListener('statechange', () => {
                    console.log('Service Worker state changed to:', newWorker.state);
                    
                    if (newWorker.state === 'installed') {
                        if (navigator.serviceWorker.controller) {
                            // Nueva versión disponible
                            console.log('New version available');
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
        });

        // Escuchar cambios en el controlador
        navigator.serviceWorker.addEventListener('controllerchange', () => {
            if (this.refreshing) return;
            console.log('Service Worker controller changed');
            this.refreshing = true;
            this.reloadApp();
        });

        // Escuchar mensajes del service worker
        navigator.serviceWorker.addEventListener('message', (event) => {
            if (event.data && event.data.type === 'SKIP_WAITING') {
                console.log('Service Worker skip waiting message received');
            }
        });

        // Verificar actualizaciones periódicamente (cada 5 minutos)
        setInterval(() => {
            this.checkForUpdates();
        }, 5 * 60 * 1000);

        // Verificar al recuperar el foco de la ventana
        window.addEventListener('focus', () => {
            setTimeout(() => {
                this.checkForUpdates();
            }, 1000);
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
            console.log('Telling service worker to skip waiting');
            this.newServiceWorker.postMessage({ command: 'skipWaiting' });
        } else if (this.registration && this.registration.waiting) {
            console.log('Telling waiting service worker to skip waiting');
            this.registration.waiting.postMessage({ command: 'skipWaiting' });
        }
    },

    reloadApp: function () {
        console.log('Reloading application...');
        window.location.reload();
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
    }
};

// Auto-inicializar el updater
document.addEventListener('DOMContentLoaded', () => {
    console.log('PWA Updater script loaded');
});