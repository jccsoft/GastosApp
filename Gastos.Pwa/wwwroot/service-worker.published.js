// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => onMessage(event));

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

// Install event - cache resources
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
        
        const cache = await caches.open(cacheName);
        
        // SOLUCIÓN: Usar addAll con manejo de errores mejorado
        try {
            await cache.addAll(assetsRequests);
            console.log(`✅ SW: ${assetsRequests.length} assets cached successfully`);
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

        // No llamar skipWaiting automáticamente - esperar a que el usuario lo solicite
        console.log('🔧 SW: Installation complete, waiting for activation signal');
        
    } catch (error) {
        console.error('❌ Service worker: Install failed:', error);
        throw error;
    }
}

// Activate event - clean up old caches
async function onActivate(event) {
    console.info('🔧 Service worker: Activate - Version:', self.assetsManifest.version);

    try {
        // Delete unused caches
        const cacheNames = await caches.keys();
        const oldCaches = cacheNames
            .filter(oldCacheName => oldCacheName.startsWith(cacheNamePrefix))
            .filter(oldCacheName => oldCacheName !== cacheName);
            
        if (oldCaches.length > 0) {
            console.log(`🧹 SW: Cleaning up ${oldCaches.length} old caches:`, oldCaches);
            await Promise.all(oldCaches.map(oldCacheName => caches.delete(oldCacheName)));
        }

        // Tomar control de todas las páginas inmediatamente
        await self.clients.claim();
        console.log('✅ SW: Activated and claimed all clients');

        // Notificar a los clientes sobre la activación
        const clients = await self.clients.matchAll();
        clients.forEach(client => {
            client.postMessage({
                type: 'SW_UPDATED',
                version: self.assetsManifest.version
            });
        });

    } catch (error) {
        console.error('❌ Service worker: Activate failed:', error);
    }
}

// Fetch event - serve from cache or network
async function onFetch(event) {
    const requestUrl = event.request.url;
    const url = new URL(requestUrl);
    
    // SOLUCIÓN: Manejar específicamente staticwebapp.config.json
    if (url.pathname === '/staticwebapp.config.json') {
        //console.log('🔧 SW: Fetching staticwebapp.config.json from network');
        return fetch(event.request, { cache: 'no-cache' }).catch(error => {
            console.warn('⚠️ SW: Failed to fetch staticwebapp.config.json:', error);
            throw error;
        });
    }
    
    // Para requests de Auth0, autenticación, o APIs, SIEMPRE ir a la red
    if (isAuth0Request(requestUrl) || isAuthenticationRoute(requestUrl) || isNetworkOnlyRoute(requestUrl)) {
        //console.log('🌐 SW: Network-only route:', url.pathname);
        
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

    let cachedResponse = null;
    
    if (event.request.method === 'GET') {
        // For navigation requests, try to serve index.html from cache
        // For API requests, try network first, then cache
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !event.request.url.includes('/api/')
            && !event.request.url.includes('/authentication/');

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
        
        // For API requests, try network first
        if (event.request.url.includes('/api/')) {
            try {
                const networkResponse = await fetch(event.request);
                // Cache successful API responses
                if (networkResponse.ok) {
                    const responseClone = networkResponse.clone();
                    cache.put(event.request, responseClone);
                }
                return networkResponse;
            } catch (error) {
                // Fall back to cache if network fails
                console.log('🌐 Network failed, serving from cache:', event.request.url);
                return cachedResponse || new Response('{"error": "Offline"}', {
                    status: 503,
                    statusText: 'Service Unavailable',
                    headers: new Headers({ 'Content-Type': 'application/json' })
                });
            }
        }
    }

    return cachedResponse || fetch(event.request);
}

// Message event - handle messages from the main thread
function onMessage(event) {
    console.log('📨 SW: Message received:', event.data);
    
    if (event.data && event.data.command) {
        switch (event.data.command) {
            case 'skipWaiting':
                console.log('🚀 SW: Skip waiting requested');
                self.skipWaiting();
                
                // Notificar al cliente que hemos saltado la espera
                event.ports[0]?.postMessage({ 
                    type: 'SKIP_WAITING_RESPONSE',
                    success: true 
                });
                break;
                
            case 'update':
                console.log('🔄 SW: Update requested');
                self.skipWaiting();
                break;
                
            case 'clear':
                console.log('🧹 SW: Clear caches requested');
                // Clear all caches
                caches.keys().then(names => {
                    return Promise.all(names.map(name => caches.delete(name)));
                }).then(() => {
                    console.log('✅ SW: All caches cleared');
                });
                break;
                
            default:
                console.log('❓ SW: Unknown command:', event.data.command);
        }
    }
}