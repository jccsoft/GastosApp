# Gestor de Tíquets de Compra

Aplicación web desarrollada con **Blazor WebApp (.NET 9)** y utilizando **MudBlazor** como framework de componentes UI. Su propósito es automatizar la lectura, interpretación y gestión de tíquets de compra mediante OCR, ofreciendo una interfaz moderna y responsiva.

## Funcionalidades principales

- 📥 **Importación de tíquets**: Acepta archivos PDF e imágenes.
- 🔍 **Procesamiento OCR**: Envía los documentos a una API externa para extraer información relevante como nombre de tienda, fecha, productos, cantidades e importes.
- 🧠 **Asignación inteligente de entidades**:
  - La primera vez que se detecta una tienda o producto, el usuario los crea manualmente.
  - En lecturas posteriores, la aplicación asocia automáticamente los datos extraídos con las entidades ya existentes.
- 🏪 **Gestión de entidades**:
  - CRUD completo para tíquets, productos y tiendas.
- 📊 **Visualización de datos**:
  - Generación de gráficos interactivos entre rangos de fechas para analizar compras realizadas.

## Tecnologías utilizadas

- Blazor WebApp (.NET 9)
- MudBlazor
- API externa de OCR
- Visual Studio