window.pwaUpdater = {
    dotNetRef: null,
    registration: null,
    refreshing: false,
    newServiceWorker: null,

    initialize: function (dotNetObjectReference) {
        this.dotNetRef = dotNetObjectReference;
        //console.log('PWA Updater initializing...');
        
        // Si ya tenemos una registración del index.html, usarla
        if (this.registration) {
            console.log('Using existing registration from index.html');
            this.setupUpdateHandlers();
        } else {
            // Si no, registrar nosotros mismos con estrategia mejorada
            this.registerServiceWorkerWithFallback().then(() => {
                this.setupUpdateHandlers();
            }).catch(error => {
                console.error('All service worker registration attempts failed:', error);
                // Crear un service worker mínimo como último recurso
                this.createMinimalServiceWorker();
            });
        }
    },

    // Estrategia mejorada de registro con múltiples intentos
    async registerServiceWorkerWithFallback() {
        if (!('serviceWorker' in navigator)) {
            console.log('Service Worker not supported');
            return Promise.resolve(null);
        }

        const isDevelopment = this.isDevelopmentEnvironment();
        
        // Lista de intentos en orden de preferencia
        const attempts = [
            {
                path: isDevelopment ? '/service-worker.js' : '/service-worker.published.js',
                description: 'Primary service worker'
            },
            {
                path: '/service-worker.js',
                description: 'Fallback to development service worker'
            },
            {
                path: '/sw-alternative.js',
                description: 'Alternative service worker'
            }
        ];

        //console.log(`🔧 Environment detected: ${isDevelopment ? 'Development' : 'Production'}`);

        for (let i = 0; i < attempts.length; i++) {
            const attempt = attempts[i];
            
            try {
                console.log(`📦 Attempt ${i + 1}: Registering ${attempt.description}: ${attempt.path}`);
                
                // Primero verificar si el archivo existe y tiene el MIME type correcto
                const fileCheck = await this.verifyServiceWorkerFile(attempt.path);
                
                if (!fileCheck.exists) {
                    console.warn(`⚠️ File ${attempt.path} does not exist, trying next...`);
                    continue;
                }
                
                if (fileCheck.wrongMimeType) {
                    console.warn(`⚠️ File ${attempt.path} has wrong MIME type (${fileCheck.contentType}), trying workaround...`);
                    
                    // Para sw-alternative.js, intentar directamente sin workaround
                    if (attempt.path === '/sw-alternative.js') {
                        console.log('⚡ Trying alternative service worker directly...');
                        const registration = await this.registerServiceWorkerDirect(attempt.path);
                        console.log(`✅ Alternative Service Worker registered successfully: ${attempt.path}`);
                        this.registration = registration;
                        return registration;
                    }
                    
                    // Intentar con query parameter para evitar cache
                    const timestamp = Date.now();
                    const pathWithTimestamp = `${attempt.path}?v=${timestamp}`;
                    
                    try {
                        const registration = await this.registerServiceWorkerDirect(pathWithTimestamp);
                        console.log(`✅ Service Worker registered with timestamp workaround: ${pathWithTimestamp}`);
                        this.registration = registration;
                        return registration;
                    } catch (timestampError) {
                        //console.warn(`⚠️ Timestamp workaround failed for ${attempt.path}:`, timestampError);
                        continue;
                    }
                }

                // Intento normal
                const registration = await this.registerServiceWorkerDirect(attempt.path);
                console.log(`✅ Service Worker registered successfully: ${attempt.path}`);
                this.registration = registration;
                return registration;
                
            } catch (error) {
                console.warn(`⚠️ Attempt ${i + 1} failed for ${attempt.path}:`, error);
                
                // Si es el último intento, propagar el error
                if (i === attempts.length - 1) {
                    throw error;
                }
            }
        }
        
        throw new Error('All service worker registration attempts failed');
    },

    // Verificar archivo del service worker antes de intentar registrarlo
    async verifyServiceWorkerFile(path) {
        try {
            const response = await fetch(path, { method: 'HEAD' });
            const contentType = response.headers.get('content-type') || '';
            
            return {
                exists: response.ok,
                status: response.status,
                contentType: contentType,
                wrongMimeType: response.ok && !contentType.includes('javascript') && !contentType.includes('application/javascript')
            };
        } catch (error) {
            return {
                exists: false,
                error: error.message
            };
        }
    },

    // Registro directo del service worker
    async registerServiceWorkerDirect(swPath) {
        return navigator.serviceWorker.register(swPath, { 
            updateViaCache: 'none',
            scope: '/' 
        });
    },

    // Crear un service worker mínimo pero funcional usando Blob URL
    async createMinimalServiceWorker() {
        console.log('🆘 Creating minimal service worker as last resort...');
        
        // Service worker mínimo que al menos permite que la PWA funcione
        const minimalSW = `
// Minimal Service Worker for PWA functionality - Version 48
console.log('🔧 Minimal Service Worker starting...');

const CACHE_NAME = 'minimal-pwa-cache-v48';
const ESSENTIAL_ASSETS = [
    '/',
    '/index.html'
];

self.addEventListener('install', (event) => {
    console.log('🔧 Minimal SW: Installing...');
    
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('🔧 Minimal SW: Caching essential assets');
                return cache.addAll(ESSENTIAL_ASSETS).catch(err => {
                    console.warn('🔧 Minimal SW: Failed to cache some assets:', err);
                });
            })
            .then(() => {
                console.log('🔧 Minimal SW: Installed successfully');
                self.skipWaiting();
            })
    );
});

self.addEventListener('activate', (event) => {
    console.log('🔧 Minimal SW: Activating...');
    
    event.waitUntil(
        Promise.all([
            // Limpiar caches antiguos
            caches.keys().then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(cacheName => cacheName !== CACHE_NAME)
                        .map(cacheName => caches.delete(cacheName))
                );
            }),
            // Tomar control inmediatamente
            self.clients.claim()
        ]).then(() => {
            console.log('🔧 Minimal SW: Activated successfully');
            
            // Notificar a los clientes
            self.clients.matchAll().then(clients => {
                clients.forEach(client => {
                    client.postMessage({
                        type: 'SW_UPDATED',
                        version: 'minimal-v48'
                    });
                });
            });
        })
    );
});

self.addEventListener('fetch', (event) => {
    // Solo interceptar requests de navegación para SPA
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request)
                .catch(() => {
                    // Si falla la red, servir desde cache
                    return caches.match('/index.html')
                        .then(response => {
                            return response || new Response('Offline', {
                                status: 503,
                                statusText: 'Service Unavailable'
                            });
                        });
                })
        );
    }
    
    // Para otros requests, ir directo a la red
});

self.addEventListener('message', (event) => {
    console.log('🔧 Minimal SW: Message received:', event.data);
    
    if (event.data && event.data.command) {
        switch (event.data.command) {
            case 'skipWaiting':
                console.log('🔧 Minimal SW: Skip waiting requested');
                self.skipWaiting();
                break;
            case 'update':
                console.log('🔧 Minimal SW: Update requested');
                self.skipWaiting();
                break;
        }
    }
});

console.log('🔧 Minimal Service Worker loaded successfully - v48');
        `;
        
        try {
            // Crear blob URL para el service worker
            const blob = new Blob([minimalSW], { type: 'application/javascript' });
            const swUrl = URL.createObjectURL(blob);
            
            const registration = await navigator.serviceWorker.register(swUrl, { 
                updateViaCache: 'none',
                scope: '/' 
            });
            
            console.log('✅ Minimal Service Worker registered successfully');
            this.registration = registration;
            this.setupUpdateHandlers();
            return registration;
            
        } catch (error) {
            console.error('❌ Even minimal service worker failed:', error);
            throw error;
        }
    },

    registerServiceWorker: function() {
        // Esta función se mantiene por compatibilidad pero ahora usa la estrategia mejorada
        return this.registerServiceWorkerWithFallback();
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
        
        //console.log(`🔍 Environment detection:`, {
        //    hostname,
        //    protocol,
        //    port,
        //    isLocalhost,
        //    isDevelopmentPort,
        //    isAzurePreview,
        //    final: isDev
        //});

        return isDev;
    },

    // Verificar si los archivos del service worker existen
    async checkServiceWorkerFiles() {
        const files = [
            '/service-worker.js',
            '/service-worker.published.js',
            '/service-worker-assets.js',
            '/sw-alternative.js'
        ];
        
        const results = {};
        
        for (const file of files) {
            try {
                const response = await fetch(file, { method: 'HEAD' });
                const contentType = response.headers.get('content-type') || '';
                
                results[file] = {
                    exists: response.ok,
                    status: response.status,
                    contentType: contentType,
                    cacheControl: response.headers.get('cache-control'),
                    wrongMimeType: response.ok && !contentType.includes('javascript') && contentType !== 'application/javascript',
                    isHTML: response.ok && contentType.includes('text/html')
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

        //console.log('Setting up update handlers...');
        //console.log('Service Worker ready, current version:', this.registration.active?.scriptURL);
        
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
                             5 * 60 * 1000;   // 5 minutos en producción

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
        //console.log(`🔍 [${env}] Checking for service worker updates...`);
        
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
    
    // Exponer funciones globales para debugging
    window.clearPWACache = () => window.pwaUpdater.clearAllCaches();
    window.checkPWAUpdates = () => window.pwaUpdater.forceUpdateCheck();
    window.getPWAInfo = () => window.pwaUpdater.getServiceWorkerInfo();
    window.forceSWUpdate = () => window.pwaUpdater.skipWaiting();
    window.checkSWFiles = () => window.pwaUpdater.checkServiceWorkerFiles();
    
    // Funciones adicionales para desarrollo
    window.enablePWADevNotifications = () => window.pwaUpdater.enableDevNotifications();
    window.disablePWADevNotifications = () => window.pwaUpdater.disableDevNotifications();

    console.groupCollapsed('🛠️ PWA Updater Debug functions loaded:');
    console.log('- clearPWACache()');
    console.log('- checkPWAUpdates()');
    console.log('- getPWAInfo()');
    console.log('- forceSWUpdate()');
    console.log('- checkSWFiles()');
    
    if (window.pwaUpdater.isDevelopmentEnvironment()) {
        console.log('🔧 Development functions: enablePWADevNotifications(), disablePWADevNotifications()');
    }

    console.groupEnd();
});