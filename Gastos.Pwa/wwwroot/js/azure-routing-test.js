// Test específico para Azure Static Web Apps routing
(async function testAzureRouting() {

    // 1. Test directo de rutas de autenticación
    const testRoutes = [
        '/authentication/login-callback',
        '/authentication/logout-callback',
        '/authentication/login-failed',
        '/authentication/logout-failed'
    ];

    for (const route of testRoutes) {
        try {
            const response = await fetch(route, { 
                method: 'GET',
                cache: 'no-cache',
                headers: {
                    'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8'
                }
            });
            
            const contentType = response.headers.get('content-type');
            const isHTML = contentType && contentType.includes('text/html');

            if (!response.ok) {
                console.warn(`❌ ${route} not served correctly.`, {
                    status: response.status,
                    statusText: response.statusText,
                    contentType: contentType,
                    isHTML: isHTML,
                    url: response.url,
                    redirected: response.redirected
                });
            }
            
            // Si es HTML, verificar que sea index.html (comportamiento esperado)
            if (isHTML && response.ok) {
                const text = await response.text();
                const isIndexHtml = text.includes('<div id="app">') && text.includes('blazor.webassembly.js');
                //console.log(`  📄 Content is index.html: ${isIndexHtml ? '✅' : '❌'}`);
                
                if (!isIndexHtml) {
                    console.log(`  📄 Content preview:`, text.substring(0, 100) + '...');
                }
            }
            
        } catch (error) {
            console.error(`❌ ${route} error:`, error.message);
        }
    }
    
    // 2. Test específico con query parameters (como lo haría Auth0)
    const callbackWithParams = '/authentication/login-callback?code=test&state=test';
    try {
        const response = await fetch(callbackWithParams, { 
            method: 'GET',
            cache: 'no-cache' 
        });

        if (!response.ok) {
            console.warn(`❌ Callback with params not served correctly:`, {
                url: callbackWithParams,
                status: response.status,
                finalUrl: response.url,
                redirected: response.redirected
            });
        }
    } catch (error) {
        console.error('❌ Callback with params error:', error);
    }
    
    // 3. Test de archivos que SÍ deberían servirse correctamente
    const staticAssets = [
        '/favicon.ico',
        '/manifest.json',
        '/_framework/blazor.webassembly.js'
    ];
    
    for (const asset of staticAssets) {
        try {
            const response = await fetch(asset, { method: 'HEAD', cache: 'no-cache' });
            if (!response.ok) {
                console.warn(`❌ ${asset} not served correctly. Status: ${response.status} ${response.statusText}`);
            }
        } catch (error) {
            console.error(`❌ ${asset} error:`, error.message);
        }
    }
})();

// Export para uso manual
window.testAzureRouting = async () => {
    // Re-ejecutar el test
    eval(document.currentScript.textContent);
};

// Test específico solo para el archivo de configuración
window.testConfigFile = async () => {
    console.log('🔧 Testing staticwebapp.config.json specifically...');
    
    try {
        // Test con diferentes métodos y headers
        const methods = [
            { method: 'GET', headers: { 'Accept': 'application/json' } },
            { method: 'GET', headers: { 'Accept': '*/*' } },
            { method: 'HEAD', headers: {} }
        ];
        
        for (const { method, headers } of methods) {
            const response = await fetch('/staticwebapp.config.json', {
                method,
                cache: 'no-cache',
                headers
            });
            
            console.log(`${method} request:`, {
                status: response.status,
                contentType: response.headers.get('content-type'),
                cacheControl: response.headers.get('cache-control'),
                lastModified: response.headers.get('last-modified')
            });
            
            if (method === 'GET') {
                const text = await response.text();
                const isJSON = text.trim().startsWith('{');
                const isHTML = text.includes('<!DOCTYPE');
                
                console.log(`  Content type: ${isJSON ? 'JSON ✅' : isHTML ? 'HTML ❌' : 'Unknown ❓'}`);
                console.log(`  Content preview:`, text.substring(0, 100));
            }
        }
    } catch (error) {
        console.error('Error testing config file:', error);
    }
};