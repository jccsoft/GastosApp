// Network status detection for PWA
let dotNetHelper;

export function initialize(dotNetHelperRef) {
    dotNetHelper = dotNetHelperRef;
    
    // Listen for online/offline events
    window.addEventListener('online', handleNetworkChange);
    window.addEventListener('offline', handleNetworkChange);
    
    // Initial status check
    handleNetworkChange();
}

export function isOnline() {
    return navigator.onLine;
}

function handleNetworkChange() {
    const isOnline = navigator.onLine;
    console.log(`Network status changed: ${isOnline ? 'online' : 'offline'}`);
    
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('OnNetworkStatusChanged', isOnline);
    }
}

export function dispose() {
    window.removeEventListener('online', handleNetworkChange);
    window.removeEventListener('offline', handleNetworkChange);
    dotNetHelper = null;
}