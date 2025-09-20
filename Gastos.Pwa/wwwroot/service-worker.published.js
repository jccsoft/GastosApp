// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => onMessage(event));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.svg$/ ];
const offlineAssetsExclude = [ /^service-worker\.js$/ ];

// Install event - cache resources
async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

// Activate event - clean up old caches
async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames
        .filter(oldCacheName => oldCacheName.startsWith(cacheNamePrefix))
        .filter(oldCacheName => oldCacheName !== cacheName)
        .map(oldCacheName => caches.delete(oldCacheName)));
}

// Fetch event - serve from cache or network
async function onFetch(event) {
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
                console.log('Network failed, serving from cache:', event.request.url);
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
    if (event.data && event.data.command) {
        switch (event.data.command) {
            case 'update':
                // Force update the cache
                self.skipWaiting();
                break;
            case 'clear':
                // Clear all caches
                caches.keys().then(names => {
                    names.forEach(name => caches.delete(name));
                });
                break;
        }
    }
}