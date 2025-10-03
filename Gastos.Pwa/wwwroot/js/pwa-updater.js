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
            }).catch(error => {
                console.error('Failed to register service worker:', error);
                // Intentar con fallback si falla el registro principal
                this.tryFallbackRegistration();
            });
        }
    },

    registerServiceWorker: function() {
        if (!('serviceWorker' in navigator)) {
            console.log('Service Worker not supported');
            return Promise.resolve(null);
        }

        // Detectar entorno mejorado - considerando PWA instalada y diferentes contextos
        const isDevelopment = this.isDevelopmentEnvironment();
        const swPath = isDevelopment ? '/service-worker.js' : '/service-worker.published.js';
        
        console.log(`🔧 Environment detected: ${isDevelopment ? 'Development' : 'Production'}`);
        console.log(`📦 Registering service worker: ${swPath}`);

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
            console.error(`❌ Service Worker registration failed (${swPath}):`, error);
            throw error;
        });
    },

    // Intentar registro con fallback si el principal falla
    tryFallbackRegistration: function() {
        console.log('🔄 Attempting fallback service worker registration...');
        
        const isDevelopment = this.isDevelopmentEnvironment();
        
        // En producción, si falla service-worker.published.js, intentar con service-worker.js
        if (!isDevelopment) {
            console.log('📦 Trying fallback: /service-worker.js');
            
            return navigator.serviceWorker.register('/service-worker.js', { 
                updateViaCache: 'none',
                scope: '/' 
            })
            .then(registration => {
                console.log('✅ Fallback Service Worker registered successfully:', registration.scope);
                this.registration = registration;
                this.setupUpdateHandlers();
                return registration;
            })
            .catch(error => {
                console.error('❌ Fallback service worker registration also failed:', error);
                // Si ambos fallan, crear un service worker mínimo
                this.createMinimalServiceWorker();
            });
        } else {
            console.log('⚠️ Development environment - no fallback needed');
        }
    },

    // Crear un service worker mínimo si no existe ninguno
    createMinimalServiceWorker: function() {
        console.log('🆘 Creating minimal service worker as last resort...');
        
        // Crear un service worker básico que al menos permita que la PWA funcione
        const minimalSW = `
            self.addEventListener('install', () => {
                console.log('Minimal SW: Installed');
                self.skipWaiting();
            });
            
            self.addEventListener('activate', () => {
                console.log('Minimal SW: Activated');
                self.clients.claim();
            });
            
            self.addEventListener('fetch', (event) => {
                // Solo interceptar requests de navegación
                if (event.request.mode === 'navigate') {
                    event.respondWith(fetch(event.request).catch(() => {
                        return caches.match('/index.html');
                    }));
                }
            });
        `;
        
        // Crear blob URL para el service worker
        const blob = new Blob([minimalSW], { type: 'application/javascript' });
        const swUrl = URL.createObjectURL(blob);
        
        return navigator.serviceWorker.register(swUrl, { 
            updateViaCache: 'none',
            scope: '/' 
        })
        .then(registration => {
            console.log('✅ Minimal Service Worker registered successfully');
            this.registration = registration;
            this.setupUpdateHandlers();
            return registration;
        })
        .catch(error => {
            console.error('❌ Even minimal service worker failed:', error);
        });
    },

    // Función mejorada para detectar entorno de desarrollo
    isDevelopmentEnvironment: function() {
        const hostname = window.location.hostname;
        const protocol = window.location.protocol;
        const port = window.location.port;
        
        // Detectar localhost en diferentes formas
        const isLocalhost = hostname === 'localhost' || 
                          hostname === '127.0.0.1' || 
                          hostname === '0.0.0.0' ||
                          hostname.includes('localhost');

        // Detectar puertos de desarrollo comunes
        const isDevelopmentPort = port && (
            port === '5000' || 
            port === '5001' || 
            port === '7000' || 
            port === '7001' ||
            port.startsWith('44') // HTTPS development ports
        );

        // Detectar URLs de Azure Static Web Apps preview
        const isAzurePreview = hostname.includes('--') && hostname.includes('.azurestaticapps.net');

        const isDev = isLocalhost || isDevelopmentPort || isAzurePreview;
        
        console.log(`🔍 Environment detection:`, {
            hostname,
            protocol,
            port,
            isLocalhost,
            isDevelopmentPort,
            isAzurePreview,
            final: isDev
        });

        return isDev;
    },

    // Verificar si los archivos del service worker existen
    async checkServiceWorkerFiles() {
        const files = [
            '/service-worker.js',
            '/service-worker.published.js',
            '/service-worker-assets.js'
        ];
        
        const results = {};
        
        for (const file of files) {
            try {
                const response = await fetch(file, { method: 'HEAD' });
                results[file] = {
                    exists: response.ok,
                    status: response.status,
                    contentType: response.headers.get('content-type')
                };
            } catch (error) {
                results[file] = {
                    exists: false,
                    error: error.message
                };
            }
        }
        
        console.log('📁 Service Worker files check:', results);
        return results;
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
            
            // Solo mostrar notificación en producción o si no hay controlador activo
            if (!this.isDevelopmentEnvironment() || !navigator.serviceWorker.controller) {
                this.showUpdateAvailable();
            }
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
                        
                        // En desarrollo, solo notificar si está explícitamente habilitado
                        // En producción, siempre notificar
                        const shouldNotify = !this.isDevelopmentEnvironment() || 
                                           this.isUpdateNotificationEnabledInDev();
                                           
                        if (shouldNotify) {
                            this.showUpdateAvailable();
                        } else {
                            console.log('🚫 Update notification suppressed in development environment');
                        }
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

        // Configurar intervalos de verificación basados en el entorno
        const updateInterval = this.isDevelopmentEnvironment() ? 
                             30 * 1000 :      // 30 segundos en desarrollo
                             2 * 60 * 1000;   // 2 minutos en producción

        const intervalId = setInterval(() => {
            this.checkForUpdates();
        }, updateInterval);

        console.log(`⏰ Update check interval set to ${updateInterval/1000} seconds`);

        // Limpiar interval si la página se descarga
        window.addEventListener('beforeunload', () => {
            clearInterval(intervalId);
        });

        // Verificar al recuperar el foco de la ventana (más agresivo en producción)
        window.addEventListener('focus', () => {
            const delay = this.isDevelopmentEnvironment() ? 2000 : 1000;
            setTimeout(() => {
                console.log('🎯 Window focused, checking for updates...');
                this.checkForUpdates();
            }, delay);
        });

        // Verificar cuando la conexión vuelve a estar disponible
        window.addEventListener('online', () => {
            setTimeout(() => {
                console.log('🌐 Connection restored, checking for updates...');
                this.checkForUpdates();
            }, 2000);
        });

        // Verificación inicial (más rápida en producción)
        const initialDelay = this.isDevelopmentEnvironment() ? 10000 : 5000;
        setTimeout(() => {
            console.log('🚀 Initial update check...');
            this.checkForUpdates();
        }, initialDelay);

        console.log('✅ Update handlers configured successfully');
    },

    // Verificar si las notificaciones de actualización están habilitadas en desarrollo
    isUpdateNotificationEnabledInDev: function() {
        // Permitir habilitar notificaciones en desarrollo mediante localStorage
        return localStorage.getItem('pwa-dev-notifications') === 'true' ||
               sessionStorage.getItem('pwa-dev-notifications') === 'true' ||
               window.location.search.includes('pwa-notifications=true');
    },

    checkForUpdates: function () {
        if (!this.registration) {
            console.log('⚠️ No service worker registration available for update check');
            return Promise.resolve(false);
        }

        const env = this.isDevelopmentEnvironment() ? 'DEV' : 'PROD';
        console.log(`🔍 [${env}] Checking for service worker updates...`);
        
        return this.registration.update().then(() => {
            console.log(`✅ [${env}] Update check completed`);
            return true;
        }).catch((error) => {
            console.error(`❌ [${env}] Error checking for updates:`, error);
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
        const env = this.isDevelopmentEnvironment() ? 'DEV' : 'PROD';
        console.log(`📢 [${env}] Notifying .NET about update available`);
        
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
                environment: this.isDevelopmentEnvironment() ? 'Development' : 'Production',
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
                controllerURL: navigator.serviceWorker.controller?.scriptURL,
                location: {
                    hostname: window.location.hostname,
                    protocol: window.location.protocol,
                    port: window.location.port,
                    origin: window.location.origin
                },
                pwaMode: window.matchMedia('(display-mode: standalone)').matches ? 'Standalone' : 'Browser',
                devNotificationsEnabled: this.isUpdateNotificationEnabledInDev()
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
        const env = this.isDevelopmentEnvironment() ? 'DEV' : 'PROD';
        console.log(`🔄 [${env}] Forcing update check...`);
        
        return this.checkForUpdates().then(() => {
            if (this.registration && this.registration.waiting) {
                console.log(`✅ [${env}] Update available after force check`);
                
                // Forzar notificación incluso en desarrollo
                this.showUpdateAvailable();
                return true;
            }
            console.log(`ℹ️ [${env}] No updates found after force check`);
            return false;
        });
    },

    // Habilitar notificaciones en desarrollo
    enableDevNotifications: function() {
        localStorage.setItem('pwa-dev-notifications', 'true');
        console.log('✅ Development notifications enabled');
    },

    // Deshabilitar notificaciones en desarrollo
    disableDevNotifications: function() {
        localStorage.removeItem('pwa-dev-notifications');
        sessionStorage.removeItem('pwa-dev-notifications');
        console.log('❌ Development notifications disabled');
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
    window.checkSWFiles = () => window.pwaUpdater.checkServiceWorkerFiles();
    
    // Funciones adicionales para desarrollo
    window.enablePWADevNotifications = () => window.pwaUpdater.enableDevNotifications();
    window.disablePWADevNotifications = () => window.pwaUpdater.disableDevNotifications();
    
    console.log('🛠️ PWA Debug functions available: clearPWACache(), checkPWAUpdates(), getPWAInfo(), forceSWUpdate(), checkSWFiles()');
    
    if (window.pwaUpdater.isDevelopmentEnvironment()) {
        console.log('🔧 Development functions: enablePWADevNotifications(), disablePWADevNotifications()');
    }
});