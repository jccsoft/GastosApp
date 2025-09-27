// Test específico para Azure Static Web Apps routing
(async function testAzureRouting() {
    console.log('🔧 === TESTING AZURE STATIC WEB APPS ROUTING ===');
    
    // 1. Verificar que staticwebapps.config.json existe y es válido
    try {
        const configResponse = await fetch('/staticwebapps.config.json', { cache: 'no-cache' });
        if (configResponse.ok) {
            const config = await configResponse.json();
            console.log('✅ staticwebapps.config.json loaded:', config);
            
            // Verificar rutas de autenticación
            const authRoutes = config.routes?.filter(r => r.route.includes('authentication')) || [];
            console.log(`📋 Found ${authRoutes.length} authentication routes:`, authRoutes);
        } else {
            console.error('❌ staticwebapps.config.json not found or invalid:', configResponse.status);
        }
    } catch (error) {
        console.error('❌ Error loading staticwebapps.config.json:', error);
    }
    
    // 2. Test directo de rutas de autenticación
    const testRoutes = [
        '/authentication/login-callback',
        '/authentication/logout-callback',
        '/authentication/login-failed',
        '/authentication/logout-failed'
    ];
    
    console.log('🔍 Testing authentication routes...');
    
    for (const route of testRoutes) {
        try {
            const response = await fetch(route, { 
                method: 'GET', // Cambiar a GET para ver el contenido
                cache: 'no-cache' 
            });
            
            const contentType = response.headers.get('content-type');
            const isHTML = contentType && contentType.includes('text/html');
            
            console.log(`${response.ok ? '✅' : '❌'} ${route}:`, {
                status: response.status,
                statusText: response.statusText,
                contentType: contentType,
                isHTML: isHTML,
                url: response.url
            });
            
            // Si es HTML, verificar que sea index.html
            if (isHTML && response.ok) {
                const text = await response.text();
                const isIndexHtml = text.includes('<div id="app">') && text.includes('blazor.webassembly.js');
                console.log(`  📄 Content is index.html: ${isIndexHtml ? '✅' : '❌'}`);
            }
            
        } catch (error) {
            console.error(`❌ ${route} error:`, error.message);
        }
    }
    
    // 3. Test específico con query parameters (como lo haría Auth0)
    const callbackWithParams = '/authentication/login-callback?code=test&state=test';
    try {
        const response = await fetch(callbackWithParams, { 
            method: 'GET',
            cache: 'no-cache' 
        });
        
        console.log(`${response.ok ? '✅' : '❌'} Callback with params:`, {
            url: callbackWithParams,
            status: response.status,
            finalUrl: response.url
        });
    } catch (error) {
        console.error('❌ Callback with params error:', error);
    }
    
    console.log('🔧 === AZURE ROUTING TEST COMPLETE ===');
})();

// Export para uso manual
window.testAzureRouting = async () => {
    // Re-ejecutar el test
    eval(document.currentScript.textContent);
};