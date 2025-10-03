// PWA Diagnostics and Debugging Tools
// This file provides comprehensive diagnostic tools for PWA troubleshooting

(function() {
    'use strict';
    
    const isDevelopment = window.location.hostname === 'localhost' ||
        window.location.hostname === '127.0.0.1' ||
        window.location.hostname.includes('localhost');

    // Diagnose application state
    window.diagnoseApp = async function() {
        console.groupCollapsed('🔍 PWA Application Diagnostics');
        
        // Basic browser support
        console.log('🌐 Browser Support:');
        console.log('  Service Worker:', 'serviceWorker' in navigator ? '✅ Supported' : '❌ Not supported');
        console.log('  Cache API:', 'caches' in window ? '✅ Available' : '❌ Not available');
        console.log('  Push API:', 'PushManager' in window ? '✅ Available' : '❌ Not available');
        console.log('  Notifications:', 'Notification' in window ? '✅ Available' : '❌ Not available');
        
        // PWA mode
        const isStandalone = window.matchMedia('(display-mode: standalone)').matches;
        const isInWebAppiOS = (window.navigator.standalone == true);
        const isInWebAppChrome = window.matchMedia('(display-mode: standalone)').matches;
        
        console.log('📱 PWA Status:');
        console.log('  Display Mode:', isStandalone ? '✅ Standalone (PWA)' : '🌐 Browser');
        console.log('  iOS Web App:', isInWebAppiOS ? '✅ Yes' : '❌ No');
        console.log('  Chrome Web App:', isInWebAppChrome ? '✅ Yes' : '❌ No');
        
        // Service Worker status
        if ('serviceWorker' in navigator) {
            console.log('🔧 Service Worker Status:');
            
            try {
                const registrations = await navigator.serviceWorker.getRegistrations();
                console.log('  Registrations Found:', registrations.length);
                
                registrations.forEach((reg, index) => {
                    console.log(`  SW ${index + 1}:`, {
                        scope: reg.scope,
                        active: !!reg.active,
                        waiting: !!reg.waiting,
                        installing: !!reg.installing,
                        updateViaCache: reg.updateViaCache
                    });
                    
                    if (reg.active) {
                        console.log(`    Active SW URL: ${reg.active.scriptURL}`);
                        console.log(`    Active SW State: ${reg.active.state}`);
                    }
                    
                    if (reg.waiting) {
                        console.log(`    Waiting SW URL: ${reg.waiting.scriptURL}`);
                        console.log(`    ⚠️ Update available but not activated`);
                    }
                });
                
                const controller = navigator.serviceWorker.controller;
                if (controller) {
                    console.log('  Current Controller:', controller.scriptURL);
                    console.log('  Controller State:', controller.state);
                } else {
                    console.log('  ❌ No active controller');
                }
            } catch (error) {
                console.error('  Error getting SW registrations:', error);
            }
        }
        
        // Cache information
        if ('caches' in window) {
            console.log('💾 Cache Status:');
            try {
                const cacheNames = await caches.keys();
                console.log('  Cache Names:', cacheNames);
                
                for (const cacheName of cacheNames) {
                    const cache = await caches.open(cacheName);
                    const keys = await cache.keys();
                    console.log(`  ${cacheName}: ${keys.length} items`);
                }
            } catch (error) {
                console.error('  Error accessing caches:', error);
            }
        }
        
        // Environment info
        console.log('🌍 Environment:');
        console.log('  URL:', window.location.href);
        console.log('  Hostname:', window.location.hostname);
        console.log('  Protocol:', window.location.protocol);
        console.log('  User Agent:', navigator.userAgent);
        console.log('  Online:', navigator.onLine ? '✅ Online' : '❌ Offline');
        console.log('  Development Mode:', isDevelopment ? '✅ Yes' : '❌ No');
        
        // Check for available tools
        console.log('🛠️ Available Tools:');
        const tools = [];
        if (typeof window.pwaUpdater !== 'undefined') tools.push('PWA Updater');
        if (typeof window.PWAInstaller !== 'undefined') tools.push('PWA Installer');
        if (typeof window.AuthDebugger !== 'undefined') tools.push('Auth Debugger');
        if (typeof window.clearPWACache !== 'undefined') tools.push('Clear Cache');
        if (typeof window.checkPWAUpdates !== 'undefined') tools.push('Check Updates');
        
        console.log('  Available:', tools.length > 0 ? tools.join(', ') : 'None');
        
        console.groupEnd();
        
        // Return summary object
        return {
            serviceWorkerSupported: 'serviceWorker' in navigator,
            cacheAPIAvailable: 'caches' in window,
            isPWA: isStandalone,
            isOnline: navigator.onLine,
            isDevelopment: isDevelopment,
            toolsAvailable: tools
        };
    };
    
    // Clear all PWA caches
    window.clearPWACache = async function() {
        console.log('🧹 Clearing all PWA caches...');
        
        if (!('caches' in window)) {
            console.warn('Cache API not available');
            return false;
        }
        
        try {
            const cacheNames = await caches.keys();
            console.log(`Found ${cacheNames.length} caches to clear:`, cacheNames);
            
            const deletePromises = cacheNames.map(cacheName => {
                console.log(`Deleting cache: ${cacheName}`);
                return caches.delete(cacheName);
            });
            
            await Promise.all(deletePromises);
            console.log('✅ All caches cleared successfully');
            
            // Also try to clear service worker if possible
            if ('serviceWorker' in navigator) {
                const registrations = await navigator.serviceWorker.getRegistrations();
                for (const registration of registrations) {
                    console.log('Unregistering service worker:', registration.scope);
                    await registration.unregister();
                }
            }
            
            console.log('🔄 Reloading page...');
            window.location.reload(true);
            
            return true;
        } catch (error) {
            console.error('❌ Error clearing caches:', error);
            return false;
        }
    };
    
    // Check for PWA updates manually
    window.checkPWAUpdates = async function() {
        console.log('🔍 Manually checking for PWA updates...');
        
        if (!('serviceWorker' in navigator)) {
            console.warn('Service Worker not supported');
            return false;
        }
        
        try {
            const registrations = await navigator.serviceWorker.getRegistrations();
            
            if (registrations.length === 0) {
                console.log('No service worker registrations found');
                return false;
            }
            
            let updateFound = false;
            
            for (const registration of registrations) {
                console.log(`Checking for updates in scope: ${registration.scope}`);
                
                const updateResult = await registration.update();
                console.log('Update check completed for:', registration.scope);
                
                if (registration.waiting) {
                    console.log('✅ Update found! Service worker is waiting to activate');
                    updateFound = true;
                } else if (registration.installing) {
                    console.log('⏳ Update installing...');
                    updateFound = true;
                } else {
                    console.log('No updates found for this registration');
                }
            }
            
            if (!updateFound) {
                console.log('No updates found in any registration');
            }
            
            return updateFound;
        } catch (error) {
            console.error('❌ Error checking for updates:', error);
            return false;
        }
    };
    
    // Get detailed PWA information
    window.getPWAInfo = async function() {
        if (!('serviceWorker' in navigator)) {
            return { 
                supported: false,
                error: 'Service Worker not supported'
            };
        }
        
        try {
            const registrations = await navigator.serviceWorker.getRegistrations();
            const cacheNames = 'caches' in window ? await caches.keys() : [];
            
            const info = {
                supported: true,
                registrations: registrations.map(reg => ({
                    scope: reg.scope,
                    active: !!reg.active,
                    waiting: !!reg.waiting,
                    installing: !!reg.installing,
                    updateViaCache: reg.updateViaCache,
                    activeScriptURL: reg.active?.scriptURL,
                    waitingScriptURL: reg.waiting?.scriptURL
                })),
                controller: navigator.serviceWorker.controller ? {
                    scriptURL: navigator.serviceWorker.controller.scriptURL,
                    state: navigator.serviceWorker.controller.state
                } : null,
                caches: cacheNames,
                displayMode: window.matchMedia('(display-mode: standalone)').matches ? 'standalone' : 'browser',
                online: navigator.onLine,
                isDevelopment: isDevelopment
            };
            
            console.log('📊 PWA Information:', info);
            return info;
        } catch (error) {
            console.error('Error getting PWA info:', error);
            return {
                supported: true,
                error: error.message
            };
        }
    };
    
    // Force service worker update
    window.forceSWUpdate = async function() {
        console.log('🚀 Forcing service worker update...');
        
        if (!('serviceWorker' in navigator)) {
            console.warn('Service Worker not supported');
            return false;
        }
        
        try {
            const registrations = await navigator.serviceWorker.getRegistrations();
            
            for (const registration of registrations) {
                if (registration.waiting) {
                    console.log('Telling waiting service worker to skip waiting...');
                    registration.waiting.postMessage({ command: 'skipWaiting' });
                    
                    // Wait for controller change
                    return new Promise((resolve) => {
                        navigator.serviceWorker.addEventListener('controllerchange', () => {
                            console.log('Controller changed, reloading...');
                            window.location.reload();
                            resolve(true);
                        }, { once: true });
                    });
                } else {
                    console.log('No waiting service worker found, checking for updates...');
                    await registration.update();
                }
            }
            
            return true;
        } catch (error) {
            console.error('❌ Error forcing SW update:', error);
            return false;
        }
    };
    
    // Initialize diagnostic tools
    function initializeDiagnostics() {
        console.log('🔧 PWA Diagnostic tools initialized');
        
        if (isDevelopment) {
            console.log('💡 Available diagnostic commands:');
            console.log('  window.diagnoseApp() - Complete diagnostic report');
            console.log('  window.clearPWACache() - Clear all caches and reload');
            console.log('  window.checkPWAUpdates() - Check for updates manually');
            console.log('  window.getPWAInfo() - Get detailed PWA information');
            console.log('  window.forceSWUpdate() - Force service worker update');
        }
        
        // Auto-diagnose issues
        setTimeout(async () => {
            if ('serviceWorker' in navigator) {
                const registrations = await navigator.serviceWorker.getRegistrations();
                const hasWaiting = registrations.some(reg => reg.waiting);
                
                if (hasWaiting) {
                    console.warn('⚠️ Service worker update detected! Run window.forceSWUpdate() to apply it.');
                }
            }
        }, 2000);
    }
    
    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeDiagnostics);
    } else {
        initializeDiagnostics();
    }
    
})();