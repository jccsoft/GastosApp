// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => {
    if (event.data && event.data.command === 'update') {
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
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames
        .filter(oldCacheName => oldCacheName.startsWith(cacheNamePrefix))
        .filter(oldCacheName => oldCacheName !== cacheName)
        .map(oldCacheName => caches.delete(oldCacheName)));
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

    // Para otros requests, usar la estrategia de cache normal
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache
        const shouldServeIndexHtml = event.request.mode === 'navigate';

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    }

    return cachedResponse || fetch(event.request).catch(error => {
        console.log('Service worker: Network request failed:', error);
        // Si es una request de navegación que falló, servir index.html
        if (event.request.mode === 'navigate') {
            return caches.open(cacheName).then(cache => 
                cache.match('index.html').then(response => 
                    response || new Response('Offline', { status: 503 })
                )
            );
        }
        throw error;
    });
}