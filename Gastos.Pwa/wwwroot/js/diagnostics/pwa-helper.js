window.pwaHelper = {
    isInstalled: function () {
        // Verifica si la app está ejecutándose como PWA instalada
        return window.matchMedia('(display-mode: standalone)').matches ||
            window.navigator.standalone === true ||
            document.referrer.includes('android-app://');
    },

    isStandalone: function () {
        // Verifica si está en modo standalone
        return window.matchMedia('(display-mode: standalone)').matches ||
            window.navigator.standalone === true;
    },

    canInstall: function () {
        // Verifica si el evento beforeinstallprompt está disponible
        return window.deferredPrompt !== undefined;
    },

    // Opcional: para manejar el evento de instalación
    setupInstallPrompt: function () {
        window.addEventListener('beforeinstallprompt', (e) => {
            e.preventDefault();
            window.deferredPrompt = e;
        });
    }
};

// Inicializar al cargar
window.pwaHelper.setupInstallPrompt();