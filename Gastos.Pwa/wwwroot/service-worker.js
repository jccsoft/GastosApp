// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => {
    if (event.data && event.data.command === 'skipWaiting') {
        console.log('Service Worker: skipWaiting command received');
        self.skipWaiting();
    }
});

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Rutas de autenticación que NUNCA deben ser cacheadas
const authRoutes = [
    '/authentication/login',
    '/authentication/logout',
    '/authentication/login-callback',
    '/authentication/logout-callback',
    '/authentication/login-failed',
    '/authentication/logout-failed'
];

// Función para verificar si una URL es de autenticación
function isAuthenticationRoute(url) {
    return authRoutes.some(route => url.includes(route));
}

// Función para verificar si una URL es de Auth0
function isAuth0Request(url) {
    return url.includes('auth0.com') || url.includes('/.well-known/openid_configuration');
}

async function onInstall(event) {
    console.info('Service worker: Install - Version:', self.assetsManifest.version);

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    
    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);

    // IMPORTANTE: No hacer skipWaiting automáticamente aquí
    // Esto permite que el usuario control cuándo aplicar la actualización
    console.info('Service worker: Install complete, waiting for activation command');
}

async function onActivate(event) {
    console.info('Service worker: Activate - Version:', self.assetsManifest.version);

    // Delete unused caches
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames
        .filter(oldCacheName => oldCacheName.startsWith(cacheNamePrefix))
        .filter(oldCacheName => oldCacheName !== cacheName)
        .map(oldCacheName => {
            console.log('Service worker: Deleting old cache:', oldCacheName);
            return caches.delete(oldCacheName);
        }));

    // Tomar control inmediato de todos los clientes
    await self.clients.claim();
    console.info('Service worker: Activated and claimed all clients');
    
    // Notificar a todos los clientes que el service worker se ha actualizado
    const clients = await self.clients.matchAll();
    clients.forEach(client => {
        client.postMessage({ type: 'SW_UPDATED', version: self.assetsManifest.version });
    });
}

async function onFetch(event) {
    const requestUrl = event.request.url;
    
    // Para requests de Auth0 o autenticación, SIEMPRE ir a la red
    if (isAuth0Request(requestUrl) || isAuthenticationRoute(requestUrl)) {
        console.log('Service worker: Auth route detected, bypassing cache:', requestUrl);
        
        // Forzar request fresco sin cache
        const freshRequest = new Request(event.request.url, {
            method: event.request.method,
            headers: event.request.headers,
            body: event.request.body,
            cache: 'no-cache',
            credentials: event.request.credentials,
            mode: event.request.mode,
            redirect: event.request.redirect,
            referrer: event.request.referrer
        });
        
        return fetch(freshRequest).catch(error => {
            console.log('Service worker: Network request failed for auth route:', error);
            // Para rutas de autenticación que fallan, servir index.html para que Blazor maneje el routing
            return caches.open(cacheName).then(cache => 
                cache.match('index.html').then(response => 
                    response || new Response('Network error', { status: 503 })
                )
            );
        });
    }

    // Para requests de navegación (HTML), aplicar estrategia de "network first" para evitar contenido obsoleto
    if (event.request.mode === 'navigate') {
        try {
            const networkResponse = await fetch(event.request);
            // Si la red responde, usar la respuesta de red y actualizar el cache
            if (networkResponse && networkResponse.status === 200) {
                const cache = await caches.open(cacheName);
                cache.put('index.html', networkResponse.clone());
                return networkResponse;
            }
        } catch (error) {
            console.log('Service worker: Network failed for navigation, falling back to cache');
        }
        
        // Si la red falla, usar el cache como respaldo
        const cache = await caches.open(cacheName);
        const cachedResponse = await cache.match('index.html');
        return cachedResponse || new Response('Offline', { status: 503 });
    }

    // Para otros requests, usar estrategia de cache first
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(event.request);
    }

    return cachedResponse || fetch(event.request).catch(error => {
        console.log('Service worker: Network request failed:', error);
        throw error;
    });
}