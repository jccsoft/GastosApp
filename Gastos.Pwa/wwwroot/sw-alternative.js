// Alternative Service Worker for Azure Static Web Apps
// This file serves as a backup when service-worker.published.js is not available

console.log('🔧 Alternative Service Worker starting...');

const CACHE_NAME = 'pwa-cache-v48';
const ESSENTIAL_ASSETS = [
    '/',
    '/index.html',
    '/manifest.webmanifest'
];

// Assets to cache (básicos para funcionamiento)
const STATIC_ASSETS = [
    '/css/app.css',
    '/css/site.css',
    '/_content/MudBlazor/MudBlazor.min.css',
    '/js/pwa-updater.js',
    '/js/pwa-update-tester.js'
];

self.addEventListener('install', (event) => {
    console.log('🔧 Alternative SW: Installing...');
    
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('🔧 Alternative SW: Caching essential assets');
                
                // Intentar cachear assets esenciales
                return cache.addAll(ESSENTIAL_ASSETS)
                    .then(() => {
                        console.log('✅ Essential assets cached');
                        // Intentar cachear assets estáticos (no críticos)
                        return Promise.allSettled(
                            STATIC_ASSETS.map(asset => cache.add(asset))
                        );
                    })
                    .then(results => {
                        const successful = results.filter(r => r.status === 'fulfilled').length;
                        const failed = results.filter(r => r.status === 'rejected').length;
                        console.log(`📦 Static assets: ${successful} cached, ${failed} failed`);
                    })
                    .catch(err => {
                        console.warn('⚠️ Some assets failed to cache:', err);
                    });
            })
            .then(() => {
                console.log('✅ Alternative SW: Installation complete');
                self.skipWaiting();
            })
            .catch(error => {
                console.error('❌ Alternative SW: Installation failed:', error);
            })
    );
});

self.addEventListener('activate', (event) => {
    console.log('🔧 Alternative SW: Activating...');
    
    event.waitUntil(
        Promise.all([
            // Limpiar caches antiguos
            caches.keys().then(cacheNames => {
                return Promise.all(
                    cacheNames
                        .filter(cacheName => cacheName.startsWith('pwa-cache-') && cacheName !== CACHE_NAME)
                        .map(cacheName => {
                            console.log(`🗑️ Cleaning old cache: ${cacheName}`);
                            return caches.delete(cacheName);
                        })
                );
            }),
            // Tomar control inmediatamente
            self.clients.claim()
        ]).then(() => {
            console.log('✅ Alternative SW: Activated successfully');
            
            // Notificar a los clientes sobre la activación
            self.clients.matchAll().then(clients => {
                clients.forEach(client => {
                    client.postMessage({
                        type: 'SW_UPDATED',
                        version: 'alternative-v48'
                    });
                });
            });
        }).catch(error => {
            console.error('❌ Alternative SW: Activation failed:', error);
        })
    );
});

self.addEventListener('fetch', (event) => {
    const url = new URL(event.request.url);
    
    // Rutas que siempre deben ir a la red
    if (url.pathname.includes('/api/') || 
        url.pathname.includes('/authentication/') || 
        url.pathname.includes('/.auth/') ||
        url.pathname.includes('/.well-known/')) {
        
        // Network first para APIs y autenticación
        event.respondWith(
            fetch(event.request).catch(error => {
                console.warn('🌐 Network failed for:', url.pathname);
                throw error;
            })
        );
        return;
    }
    
    // Para navegación (páginas de la SPA)
    if (event.request.mode === 'navigate') {
        event.respondWith(
            fetch(event.request)
                .then(response => {
                    // Si la respuesta es OK, cachearla y devolverla
                    if (response.ok) {
                        const responseClone = response.clone();
                        caches.open(CACHE_NAME).then(cache => {
                            cache.put(event.request, responseClone);
                        });
                    }
                    return response;
                })
                .catch(() => {
                    // Si falla la red, servir desde cache
                    return caches.match('/index.html')
                        .then(response => {
                            if (response) {
                                console.log('📱 Serving index.html from cache for:', url.pathname);
                                return response;
                            }
                            return new Response('Offline - No cached content available', {
                                status: 503,
                                statusText: 'Service Unavailable',
                                headers: { 'Content-Type': 'text/html' }
                            });
                        });
                })
        );
        return;
    }
    
    // Para otros recursos (CSS, JS, imágenes, etc.)
    event.respondWith(
        caches.match(event.request)
            .then(cachedResponse => {
                if (cachedResponse) {
                    // Servir desde cache
                    return cachedResponse;
                }
                
                // Si no está en cache, ir a la red
                return fetch(event.request)
                    .then(response => {
                        // Si es un recurso estático exitoso, cachearlo
                        if (response.ok && 
                            (url.pathname.endsWith('.css') || 
                             url.pathname.endsWith('.js') || 
                             url.pathname.endsWith('.png') || 
                             url.pathname.endsWith('.jpg') || 
                             url.pathname.endsWith('.svg'))) {
                            
                            const responseClone = response.clone();
                            caches.open(CACHE_NAME).then(cache => {
                                cache.put(event.request, responseClone);
                            });
                        }
                        return response;
                    })
                    .catch(error => {
                        console.warn('🌐 Failed to fetch:', url.pathname, error);
                        throw error;
                    });
            })
    );
});

self.addEventListener('message', (event) => {
    console.log('📨 Alternative SW: Message received:', event.data);
    
    if (event.data && event.data.command) {
        switch (event.data.command) {
            case 'skipWaiting':
                console.log('🚀 Alternative SW: Skip waiting requested');
                self.skipWaiting();
                
                // Responder al cliente
                if (event.ports && event.ports[0]) {
                    event.ports[0].postMessage({ 
                        type: 'SKIP_WAITING_RESPONSE',
                        success: true 
                    });
                }
                break;
                
            case 'update':
                console.log('🔄 Alternative SW: Update requested');
                self.skipWaiting();
                break;
                
            case 'clear':
                console.log('🧹 Alternative SW: Clear caches requested');
                caches.keys().then(names => {
                    return Promise.all(names.map(name => caches.delete(name)));
                }).then(() => {
                    console.log('✅ Alternative SW: All caches cleared');
                });
                break;
                
            default:
                console.log('❓ Alternative SW: Unknown command:', event.data.command);
        }
    }
});

// Manejo de errores globales
self.addEventListener('error', (event) => {
    console.error('❌ Alternative SW: Error:', event.error);
});

self.addEventListener('unhandledrejection', (event) => {
    console.error('❌ Alternative SW: Unhandled rejection:', event.reason);
});

console.log('✅ Alternative Service Worker loaded successfully - Version 48');