// Enhanced Service Worker registration with better update handling
// Based on: https://whuysentruit.medium.com/blazor-wasm-pwa-adding-a-new-update-available-notification-d9f65c4ad13

(function() {
    'use strict';
    
    console.log('📝 SW Registrator loading...');

    window.updateAvailable = new Promise((resolve, reject) => {

        console.group('Check update available');

        if (!('serviceWorker' in navigator)) {
            const errorMessage = `This browser doesn't support service workers`;
            console.error('❌ ' + errorMessage);
            reject(errorMessage);
            return;
        }

        navigator.serviceWorker.register('/service-worker.js', { 
            updateViaCache: 'none',
            scope: '/'
        })
            .then(registration => {
                console.info(`✅ Service worker registration successful (scope: ${registration.scope})`);

                // Verificar si ya hay un service worker esperando
                if (registration.waiting) {
                    console.log('⏳ Service worker waiting found immediately');
                    resolve(true);
                    return;
                }

                // Verificar actualizaciones cada 30 segundos
                const updateInterval = setInterval(() => {
                    console.log('🔍 Checking for service worker updates...');
                    registration.update().catch(err => {
                        console.warn('⚠️ Update check failed:', err);
                    });
                }, 30 * 1000);

                // Limpiar el intervalo cuando la página se descarga
                window.addEventListener('beforeunload', () => {
                    clearInterval(updateInterval);
                });

                // Escuchar cuando se encuentra una actualización
                registration.addEventListener('updatefound', () => {
                    console.log('🔄 Service worker update found');
                    const installingServiceWorker = registration.installing;
                    
                    if (!installingServiceWorker) {
                        console.warn('⚠️ Installing service worker is null');
                        return;
                    }
                    
                    installingServiceWorker.addEventListener('statechange', () => {
                        console.log(`📊 Service worker state changed to: ${installingServiceWorker.state}`);
                        
                        switch (installingServiceWorker.state) {
                            case 'installed':
                                if (navigator.serviceWorker.controller) {
                                    // Nueva versión instalada, esperando activación
                                    console.log('✅ New service worker installed, waiting for activation');
                                    resolve(true);
                                } else {
                                    // Primera instalación
                                    console.log('🆕 Service worker installed for the first time');
                                    resolve(false);
                                }
                                break;
                            case 'activated':
                                console.log('🎉 New service worker activated');
                                break;
                            case 'redundant':
                                console.warn('⚠️ Service worker became redundant');
                                break;
                        }
                    });
                });

                // Escuchar cambios en el controller (cuando se activa un nuevo service worker)
                let controllerChangeHandled = false;
                navigator.serviceWorker.addEventListener('controllerchange', () => {
                    if (controllerChangeHandled) return;
                    controllerChangeHandled = true;
                    
                    console.log('🔄 Service worker controller changed - page will reload');
                    
                    // Notificar que la página se va a recargar
                    if (window.pwaUpdater && typeof window.pwaUpdater.handleControllerChange === 'function') {
                        window.pwaUpdater.handleControllerChange();
                    } else {
                        // Fallback: recargar directamente
                        setTimeout(() => {
                            window.location.reload();
                        }, 500);
                    }
                });

                // Escuchar mensajes del service worker
                navigator.serviceWorker.addEventListener('message', (event) => {
                    if (event.data && event.data.type === 'SW_UPDATED') {
                        console.log('📨 Service worker updated to version:', event.data.version);
                        if (window.pwaUpdater && typeof window.pwaUpdater.onSwUpdated === 'function') {
                            window.pwaUpdater.onSwUpdated(event.data.version);
                        }
                    }
                });

                // Verificar inmediatamente si hay actualizaciones
                setTimeout(() => {
                    console.log('🚀 Initial service worker update check...');
                    registration.update().catch(err => {
                        console.warn('⚠️ Initial service worker update check failed:', err);
                    });
                }, 1000);
            })
            .catch(error => {
                console.error('❌ Service worker registration failed:', error);
                reject(error);
            });

        console.groupEnd();
    });

    window.registerForUpdateAvailableNotification = (caller, methodName) => {
        console.group('📋 Registering for update available notification');
        
        window.updateAvailable.then(isUpdateAvailable => {
            console.log('📊 Update available check result:', isUpdateAvailable);
            
            // Solo notificar si hay actualizaciones y no estamos en localhost
            if (isUpdateAvailable && !window.location.origin.includes('localhost')) {
                console.log('🔔 Calling update notification method:', methodName);
                caller.invokeMethodAsync(methodName)
                    .then(() => {
                        console.log('✅ Update notification sent successfully');
                    })
                    .catch(err => {
                        console.error('❌ Error calling update notification method:', err);
                    });
            } else {
                console.log('ℹ️ No update notification needed:', {
                    isUpdateAvailable,
                    isLocalhost: window.location.origin.includes('localhost')
                });
            }
        }).catch(err => {
            console.error('❌ Error in registerForUpdateAvailableNotification:', err);
        });

        console.groupEnd();
    };

    // Exponer función para obtener estado del registro
    window.getServiceWorkerRegistration = async function() {
        if (!('serviceWorker' in navigator)) {
            return null;
        }

        try {
            const registrations = await navigator.serviceWorker.getRegistrations();
            return registrations.find(reg => reg.scope === window.location.origin + '/') || registrations[0] || null;
        } catch (error) {
            console.error('Error getting service worker registration:', error);
            return null;
        }
    };

    console.log('✅ SW Registrator loaded successfully');
})();