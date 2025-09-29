// Enhanced Service Worker registration with better update handling
// Based on: https://whuysentruit.medium.com/blazor-wasm-pwa-adding-a-new-update-available-notification-d9f65c4ad13

window.updateAvailable = new Promise((resolve, reject) => {
    if (!('serviceWorker' in navigator)) {
        const errorMessage = `This browser doesn't support service workers`;
        console.error(errorMessage);
        reject(errorMessage);
        return;
    }

    navigator.serviceWorker.register('/service-worker.js', { 
        updateViaCache: 'none',
        scope: '/'
    })
        .then(registration => {
            console.info(`Service worker registration successful (scope: ${registration.scope})`);

            // Verificar si ya hay un service worker esperando
            if (registration.waiting) {
                console.log('Service worker waiting found immediately');
                resolve(true);
                return;
            }

            // Verificar actualizaciones más frecuentemente
            setInterval(() => {
                console.log('Checking for service worker updates...');
                registration.update();
            }, 30 * 1000); // 30 segundos en lugar de 60

            registration.addEventListener('updatefound', () => {
                console.log('Service worker update found');
                const installingServiceWorker = registration.installing;
                
                if (!installingServiceWorker) return;
                
                installingServiceWorker.addEventListener('statechange', () => {
                    console.log('Service worker state changed to:', installingServiceWorker.state);
                    
                    if (installingServiceWorker.state === 'installed') {
                        if (navigator.serviceWorker.controller) {
                            // Nueva versión instalada, pero esperando activación
                            console.log('New service worker installed, waiting for activation');
                            resolve(true);
                        } else {
                            // Primera instalación
                            console.log('Service worker installed for the first time');
                            resolve(false);
                        }
                    }
                });
            });

            // Escuchar cambios en el controller (cuando se activa un nuevo service worker)
            navigator.serviceWorker.addEventListener('controllerchange', () => {
                console.log('Service worker controller changed - page will reload');
                // Notificar que la página se va a recargar
                if (window.pwaUpdater && window.pwaUpdater.handleControllerChange) {
                    window.pwaUpdater.handleControllerChange();
                } else {
                    window.location.reload();
                }
            });

            // Escuchar mensajes del service worker
            navigator.serviceWorker.addEventListener('message', (event) => {
                if (event.data && event.data.type === 'SW_UPDATED') {
                    console.log('Service worker updated to version:', event.data.version);
                    if (window.pwaUpdater && window.pwaUpdater.onSwUpdated) {
                        window.pwaUpdater.onSwUpdated(event.data.version);
                    }
                }
            });

            // Verificar inmediatamente si hay actualizaciones
            registration.update().catch(err => {
                console.warn('Initial service worker update check failed:', err);
            });
        })
        .catch(error => {
            console.error('Service worker registration failed with error:', error);
            reject(error);
        });
});

window.registerForUpdateAvailableNotification = (caller, methodName) => {
    window.updateAvailable.then(isUpdateAvailable => {
        if (isUpdateAvailable && !window.location.origin.includes('localhost')) {
            caller.invokeMethodAsync(methodName).then();
        }
    }).catch(err => {
        console.error('Error in registerForUpdateAvailableNotification:', err);
    });
};