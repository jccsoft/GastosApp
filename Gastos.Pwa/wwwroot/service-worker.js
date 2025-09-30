// Enhanced Service Worker with better update handling and caching strategies
// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => {
    if (event.data && event.data.command === 'skipWaiting') {
        console.log('🚀 Service Worker: skipWaiting command received');
        self.skipWaiting();
    }
});

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ 
    /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, 
    /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, 
    /\.blat$/, /\.dat$/, /\.svg$/, /\.woff2$/, /\.ttf$/, /\.eot$/
];

// SOLUCIÓN: Excluir archivos que causan problemas de integridad
const offlineAssetsExclude = [ 
    /^service-worker\.js$/, 
    /^staticwebapp\.config\.json$/,  // ← AGREGADO: Excluir archivo de configuración
    /^appsettings.*\.json$/          // ← AGREGADO: Excluir archivos de configuración
];

// Rutas de autenticación que NUNCA deben ser cacheadas
const authRoutes = [
    '/authentication/login',
    '/authentication/logout',
    '/authentication/login-callback',
    '/authentication/logout-callback',
    '/authentication/login-failed',
    '/authentication/logout-failed'
];

// URLs que siempre deben ir a la red
const networkOnlyRoutes = [
    '/api/',
    '/.auth/',
    '/.well-known/',
    '/staticwebapp.config.json'  // ← AGREGADO: Siempre obtener de la red
];

// Función para verificar si una URL es de autenticación
function isAuthenticationRoute(url) {
    return authRoutes.some(route => url.includes(route));
}

// Función para verificar si una URL es de Auth0
function isAuth0Request(url) {
    return url.includes('auth0.com') || url.includes('/.well-known/openid_configuration');
}

// Función para verificar si debe ir solo a la red
function isNetworkOnlyRoute(url) {
    return networkOnlyRoutes.some(route => url.includes(route));
}

// Función para verificar si un asset debe omitir verificación de integridad
function shouldSkipIntegrityCheck(assetUrl) {
    return assetUrl.includes('staticwebapp.config.json') || 
           assetUrl.includes('appsettings') || 
           assetUrl.endsWith('.config.json');
}

async function onInstall(event) {
    console.info('🔧 Service worker: Install - Version:', self.assetsManifest.version);

    try {
        // Fetch and cache all matching items from the assets manifest
        const assetsRequests = self.assetsManifest.assets
            .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
            .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
            .map(asset => {
                // SOLUCIÓN: Crear request SIN integrity check para archivos problemáticos
                if (shouldSkipIntegrityCheck(asset.url)) {
                    console.log(`⚠️ SW: Skipping integrity check for: ${asset.url}`);
                    return new Request(asset.url, { cache: 'no-cache' });
                } else {
                    // Crear request con integrity check normal
                    return new Request(asset.url, { 
                        integrity: asset.hash, 
                        cache: 'no-cache' 
                    });
                }
            });
        
        console.log(`📦 Caching ${assetsRequests.length} assets...`);
        
        const cache = await caches.open(cacheName);
        
        // SOLUCIÓN: Usar addAll con manejo de errores mejorado
        try {
            await cache.addAll(assetsRequests);
        } catch (error) {
            console.warn('⚠️ SW: addAll failed, trying individual adds:', error);
            
            // Si addAll falla, intentar agregar archivos individualmente
            const results = await Promise.allSettled(
                assetsRequests.map(request => cache.add(request))
            );
            
            const failed = results.filter(r => r.status === 'rejected');
            if (failed.length > 0) {
                console.warn(`⚠️ SW: ${failed.length} assets failed to cache:`, failed);
            }
            
            const succeeded = results.filter(r => r.status === 'fulfilled');
            console.log(`✅ SW: ${succeeded.length} assets cached successfully`);
        }
        
        console.info('✅ Service worker: Install complete - assets cached');
        
        // NO hacer skipWaiting automáticamente - esperar comando del usuario
        console.info('⏳ Service worker: Waiting for activation command from user');
        
    } catch (error) {
        console.error('❌ Service worker: Install failed:', error);
        throw error;
    }
}

async function onActivate(event) {
    console.info('🎉 Service worker: Activate - Version:', self.assetsManifest.version);

    try {
        // Delete unused caches
        const cacheNames = await caches.keys();
        const oldCaches = cacheNames
            .filter(oldCacheName => oldCacheName.startsWith(cacheNamePrefix))
            .filter(oldCacheName => oldCacheName !== cacheName);

        if (oldCaches.length > 0) {
            console.log(`🗑️ Deleting ${oldCaches.length} old caches:`, oldCaches);
            await Promise.all(oldCaches.map(oldCacheName => {
                console.log(`Deleting cache: ${oldCacheName}`);
                return caches.delete(oldCacheName);
            }));
        } else {
            console.log('ℹ️ No old caches to delete');
        }

        // Tomar control inmediato de todos los clientes
        await self.clients.claim();
        console.info('✅ Service worker: Activated and claimed all clients');
        
        // Notificar a todos los clientes que el service worker se ha actualizado
        const clients = await self.clients.matchAll();
        console.log(`📨 Notifying ${clients.length} clients of update`);
        
        clients.forEach(client => {
            client.postMessage({ 
                type: 'SW_UPDATED', 
                version: self.assetsManifest.version,
                timestamp: new Date().toISOString()
            });
        });
        
    } catch (error) {
        console.error('❌ Service worker: Activate failed:', error);
        throw error;
    }
}

async function onFetch(event) {
    const requestUrl = event.request.url;
    const url = new URL(requestUrl);
    
    // SOLUCIÓN: Manejar específicamente staticwebapp.config.json
    if (url.pathname === '/staticwebapp.config.json') {
        console.log('🔧 SW: Fetching staticwebapp.config.json from network');
        return fetch(event.request, { cache: 'no-cache' }).catch(error => {
            console.warn('⚠️ SW: Failed to fetch staticwebapp.config.json:', error);
            throw error;
        });
    }
    
    // Para requests de Auth0, autenticación, o APIs, SIEMPRE ir a la red
    if (isAuth0Request(requestUrl) || isAuthenticationRoute(requestUrl) || isNetworkOnlyRoute(requestUrl)) {
        console.log('🌐 SW: Network-only route:', url.pathname);
        
        return fetch(event.request).catch(error => {
            console.warn('⚠️ SW: Network request failed for network-only route:', error);
            
            // Para rutas de navegación que fallan, servir index.html como fallback
            if (event.request.mode === 'navigate') {
                return caches.open(cacheName).then(cache => 
                    cache.match('index.html').then(response => 
                        response || new Response('Network error', { 
                            status: 503,
                            statusText: 'Service Unavailable'
                        })
                    )
                );
            }
            
            throw error;
        });
    }

    // Para navegación (HTML), usar estrategia "stale-while-revalidate"
    if (event.request.mode === 'navigate') {
        console.log('📄 SW: Navigation request for:', url.pathname);
        
        const cache = await caches.open(cacheName);
        
        // Intentar obtener de cache primero
        const cachedResponse = await cache.match('index.html');
        
        // Intentar actualizar en background
        const networkPromise = fetch(event.request)
            .then(response => {
                if (response && response.status === 200) {
                    // Actualizar cache con nueva versión
                    cache.put('index.html', response.clone());
                    console.log('🔄 SW: Updated index.html in cache');
                }
                return response;
            })
            .catch(error => {
                console.warn('⚠️ SW: Network failed for navigation:', error);
                return null;
            });

        // Devolver cache si existe, sino esperar a la red
        if (cachedResponse) {
            console.log('⚡ SW: Serving navigation from cache');
            // Actualizar en background
            networkPromise;
            return cachedResponse;
        } else {
            console.log('🌐 SW: No cache for navigation, waiting for network');
            return networkPromise.then(response => 
                response || new Response('Offline', { 
                    status: 503,
                    statusText: 'Service Unavailable'
                })
            );
        }
    }

    // Para otros recursos (CSS, JS, etc), usar cache-first
    if (event.request.method === 'GET') {
        const cache = await caches.open(cacheName);
        const cachedResponse = await cache.match(event.request);
        
        if (cachedResponse) {
            console.log('⚡ SW: Serving from cache:', url.pathname);
            return cachedResponse;
        } else {
            console.log('🌐 SW: Cache miss, fetching:', url.pathname);
            
            return fetch(event.request)
                .then(response => {
                    // Cachear la respuesta si es exitosa
                    if (response && response.status === 200 && 
                        offlineAssetsInclude.some(pattern => pattern.test(url.pathname))) {
                        cache.put(event.request, response.clone());
                        console.log('📦 SW: Cached new resource:', url.pathname);
                    }
                    return response;
                })
                .catch(error => {
                    console.warn('⚠️ SW: Network request failed:', error);
                    throw error;
                });
        }
    }

    // Para otros métodos (POST, PUT, etc), pasar directamente
    console.log('🔄 SW: Passing through non-GET request:', event.request.method, url.pathname);
    return fetch(event.request);
}

// Log de inicialización
console.log('🚀 Service Worker script loaded, version:', self.assetsManifest?.version || 'unknown');