# Gestor de Tíquets de Compra / Purchase Receipt Manager

[**Español**](#español) | [**English**](#english)

---

## Español

Aplicación web desarrollada con **Blazor WebApp (.NET 9)** y utilizando **MudBlazor** como framework de componentes UI. Su propósito es automatizar la lectura, interpretación y gestión de tíquets de compra mediante OCR, ofreciendo una interfaz moderna y responsiva.

### Funcionalidades principales

- 📥 **Importación de tíquets**: Acepta archivos PDF e imágenes.
- 🔍 **Procesamiento OCR**: Envía los documentos a una API externa para extraer información relevante como nombre de tienda, fecha, productos, cantidades e importes.
- 🧠 **Asignación inteligente de entidades**:
  - La primera vez que se detecta una tienda o producto, el usuario los crea manualmente.
  - En lecturas posteriores, la aplicación asocia automáticamente los datos extraídos con las entidades ya existentes.
- 🏪 **Gestión de entidades**:
  - CRUD completo para tíquets, productos y tiendas.
- 📊 **Visualización de datos**:
  - Generación de gráficos interactivos entre rangos de fechas para analizar compras realizadas.

### Tecnologías utilizadas

- Blazor WebApp (.NET 9)
- MudBlazor
- API externa de OCR
- Visual Studio

---

## English

A web application built with **Blazor WebApp (.NET 9)** and using **MudBlazor** as the UI component framework. Its purpose is to automate the reading, interpretation, and management of purchase receipts through OCR, offering a modern and responsive interface.

### Main Features

- 📥 **Receipt Import**: Accepts PDF files and images.
- 🔍 **OCR Processing**: Sends documents to an external API to extract relevant information such as store name, date, products, quantities, and amounts.
- 🧠 **Intelligent Entity Assignment**:
  - The first time a store or product is detected, the user creates them manually.
  - In subsequent readings, the application automatically associates the extracted data with existing entities.
- 🏪 **Entity Management**:
  - Full CRUD for receipts, products, and stores.
- 📊 **Data Visualization**:
  - Generation of interactive charts between date ranges to analyze purchases made.

### Technologies Used

- Blazor WebApp (.NET 9)
- MudBlazor
- External OCR API
- Visual Studio