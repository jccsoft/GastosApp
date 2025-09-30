// Test específico para Azure Static Web Apps routing
(async function testAzureRouting() {
    console.log('🔧 === TESTING AZURE STATIC WEB APPS ROUTING ===');
    
    // 1. Verificar que staticwebapps.config.json existe y es válido
    //try {
    //    const configResponse = await fetch('/staticwebapp.config.json', { 
    //        cache: 'no-cache',
    //        headers: {
    //            'Accept': 'application/json'
    //        }
    //    });
        
    //    if (configResponse.ok) {
    //        const contentType = configResponse.headers.get('content-type');
    //        console.log(`📄 Response content-type: ${contentType}`);
            
    //        // Verificar si la respuesta es realmente JSON
    //        if (contentType && contentType.includes('application/json')) {
    //            const config = await configResponse.json();
    //            console.log('✅ staticwebapps.config.json loaded:', config);
                
    //            // Verificar rutas de autenticación
    //            const authRoutes = config.routes?.filter(r => r.route.includes('authentication')) || [];
    //            console.log(`📋 Found ${authRoutes.length} authentication routes:`, authRoutes);
    //        } else {
    //            const responseText = await configResponse.text();
    //            if (responseText.includes('<!DOCTYPE')) {
    //                console.error('❌ staticwebapps.config.json is returning HTML instead of JSON');
    //                console.error('🔧 This indicates a routing configuration problem in Azure Static Web Apps');
    //                console.error('💡 The navigationFallback rule is catching the JSON file and serving index.html');
    //                console.log('📄 Response preview:', responseText.substring(0, 200) + '...');
    //            } else {
    //                console.error('❌ staticwebapps.config.json returned unexpected content:', responseText.substring(0, 200));
    //            }
    //        }
    //    } else {
    //        console.error('❌ staticwebapps.config.json not found or invalid:', configResponse.status);
    //    }
    //} catch (error) {
    //    if (error.message.includes('Unexpected token')) {
    //        console.error('❌ staticwebapps.config.json is being served as HTML instead of JSON');
    //        console.error('🔧 SOLUTION: Add "/staticwebapp.config.json" to the navigationFallback exclude list');
    //        console.error('💡 Or add a specific route for the config file');
    //    } else {
    //        console.error('❌ Error loading staticwebapps.config.json:', error);
    //    }
    //}
    
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
                method: 'GET',
                cache: 'no-cache',
                headers: {
                    'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8'
                }
            });
            
            const contentType = response.headers.get('content-type');
            const isHTML = contentType && contentType.includes('text/html');
            
            console.log(`${response.ok ? '✅' : '❌'} ${route}:`, {
                status: response.status,
                statusText: response.statusText,
                contentType: contentType,
                isHTML: isHTML,
                url: response.url,
                redirected: response.redirected
            });
            
            // Si es HTML, verificar que sea index.html (comportamiento esperado)
            if (isHTML && response.ok) {
                const text = await response.text();
                const isIndexHtml = text.includes('<div id="app">') && text.includes('blazor.webassembly.js');
                console.log(`  📄 Content is index.html: ${isIndexHtml ? '✅' : '❌'}`);
                
                if (!isIndexHtml) {
                    console.log(`  📄 Content preview:`, text.substring(0, 100) + '...');
                }
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
            finalUrl: response.url,
            redirected: response.redirected
        });
    } catch (error) {
        console.error('❌ Callback with params error:', error);
    }
    
    // 4. Test de archivos que SÍ deberían servirse correctamente
    console.log('🔍 Testing static assets...');
    const staticAssets = [
        '/favicon.ico',
        '/manifest.json',
        '/_framework/blazor.webassembly.js'
    ];
    
    for (const asset of staticAssets) {
        try {
            const response = await fetch(asset, { method: 'HEAD', cache: 'no-cache' });
            console.log(`${response.ok ? '✅' : '❌'} ${asset}: ${response.status} ${response.statusText}`);
        } catch (error) {
            console.error(`❌ ${asset} error:`, error.message);
        }
    }
    
    console.log('🔧 === AZURE ROUTING TEST COMPLETE ===');
    
    // Resumen de soluciones
    console.log('\n💡 === SUGGESTED SOLUTIONS ===');
    console.log('1. Add "/staticwebapp.config.json" to navigationFallback.exclude');
    console.log('2. Add "*.json" to navigationFallback.exclude');
    console.log('3. Add specific routes for JSON files');
    console.log('4. Verify mimeTypes configuration includes ".json": "application/json"');
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