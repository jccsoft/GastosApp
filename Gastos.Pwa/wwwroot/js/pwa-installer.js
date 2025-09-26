// PWA Installation Handler
let deferredPrompt;
let installButton;

// Listen for the beforeinstallprompt event
window.addEventListener('beforeinstallprompt', (e) => {
    // Prevent the mini-infobar from appearing on mobile
    e.preventDefault();
    // Stash the event so it can be triggered later
    deferredPrompt = e;
    // Update UI notify the user they can install the PWA
    showInstallPromotion();
    console.log('PWA install prompt available');
});

// Listen for the appinstalled event
window.addEventListener('appinstalled', (evt) => {
    console.log('PWA was installed');
    hideInstallPromotion();
    // Show a welcome message or redirect to a success page
    showWelcomeMessage();
});

// Function to show install promotion
function showInstallPromotion() {
    // Create install button if it doesn't exist
    if (!installButton) {
        createInstallButton();
    }
    
    // Show install button
    if (installButton) {
        installButton.style.display = 'block';
    }
}

// Function to hide install promotion
function hideInstallPromotion() {
    if (installButton) {
        installButton.style.display = 'none';
    }
}

// Function to create install button
function createInstallButton() {
    // Check if we're not already in standalone mode
    if (window.matchMedia('(display-mode: standalone)').matches) {
        return;
    }
    
    // Create the install button
    installButton = document.createElement('button');
    installButton.innerHTML = '📱 Instalar App';
    installButton.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        padding: 12px 24px;
        background: #1976d2;
        color: white;
        border: none;
        border-radius: 24px;
        cursor: pointer;
        z-index: 1000;
        font-size: 14px;
        font-weight: 500;
        box-shadow: 0 4px 8px rgba(0,0,0,0.2);
        display: none;
        animation: slideIn 0.3s ease-out;
    `;
    
    // Add click handler
    installButton.addEventListener('click', installPWA);
    
    // Add to document
    document.body.appendChild(installButton);
    
    // Add CSS animation
    if (!document.getElementById('pwa-animations')) {
        const style = document.createElement('style');
        style.id = 'pwa-animations';
        style.textContent = `
            @keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @keyframes slideInFromTop {
                from { transform: translateY(-100%); opacity: 0; }
                to { transform: translateY(0); opacity: 1; }
            }
        `;
        document.head.appendChild(style);
    }
}

// Function to install PWA
async function installPWA() {
    if (!deferredPrompt) {
        console.log('No deferred prompt available');
        return false;
    }
    
    try {
        // Show the install prompt
        deferredPrompt.prompt();
        
        // Wait for the user to respond to the prompt
        const { outcome } = await deferredPrompt.userChoice;
        
        if (outcome === 'accepted') {
            console.log('User accepted the install prompt');
        } else {
            console.log('User dismissed the install prompt');
        }
        
        // Clear the deferred prompt
        deferredPrompt = null;
        hideInstallPromotion();
        
        return outcome === 'accepted';
    } catch (error) {
        console.error('Error during PWA installation:', error);
        return false;
    }
}

// Function to show welcome message
function showWelcomeMessage() {
    // Create welcome notification
    const welcome = document.createElement('div');
    welcome.innerHTML = `
        <div style="
            position: fixed;
            top: 20px;
            right: 20px;
            background: #4caf50;
            color: white;
            padding: 16px 24px;
            border-radius: 8px;
            z-index: 1001;
            font-size: 14px;
            box-shadow: 0 4px 8px rgba(0,0,0,0.2);
            animation: slideInFromTop 0.3s ease-out;
        ">
            ✅ ¡App instalada correctamente!
        </div>
    `;
    
    document.body.appendChild(welcome);
    
    // Remove after 3 seconds
    setTimeout(() => {
        welcome.remove();
    }, 3000);
}

// Function to check PWA display mode
function getPWADisplayMode() {
    const isStandalone = window.matchMedia('(display-mode: standalone)');
    
    if (document.referrer.startsWith('android-app://')) {
        return 'twa';
    } else if (navigator.standalone || isStandalone.matches) {
        return 'standalone';
    }
    return 'browser';
}

// Function to handle authentication routes in PWA mode
function handleAuthRedirect(url) {
    if (getPWADisplayMode() === 'standalone') {
        // En modo PWA, usar window.location para navegación
        window.location.href = url;
    } else {
        // En modo browser, comportamiento normal
        return true;
    }
}

// Check if already installed and running in standalone mode
const displayMode = getPWADisplayMode();
console.log(`PWA is running in ${displayMode} mode`);

// Export functions for use in Blazor components
window.PWAInstaller = {
    canInstall: () => !!deferredPrompt,
    isInstalled: () => displayMode === 'standalone' || displayMode === 'twa',
    getDisplayMode: () => displayMode,
    install: installPWA,
    showPromotion: showInstallPromotion,
    hidePromotion: hideInstallPromotion,
    handleAuthRedirect: handleAuthRedirect
};

// Store the deferred prompt for global access
window.deferredPrompt = deferredPrompt;