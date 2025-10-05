# Sistema de Versionado Automático

Este documento explica cómo funciona el sistema de versionado automático implementado en la aplicación GastosApp, que utiliza GitHub Actions para generar versiones y releases de forma automática y manual.

## 📋 Índice

- [Visión General](#visión-general)
- [Tipos de Versionado](#tipos-de-versionado)
- [Conventional Commits](#conventional-commits)
- [Workflows de GitHub Actions](#workflows-de-github-actions)
- [Modelo de Versión](#modelo-de-versión)
- [Configuración y Permisos](#configuración-y-permisos)
- [Uso Práctico](#uso-práctico)
- [Integración con la Aplicación](#integración-con-la-aplicación)

## 🎯 Visión General

El sistema de versionado de GastosApp está diseñado para automatizar la creación de versiones y releases utilizando:

- **Versionado Semántico (SemVer)**: Formato `v1.2.3` (major.minor.patch)
- **Conventional Commits**: Para determinar automáticamente el tipo de versión
- **GitHub Actions**: Para automatizar todo el proceso
- **Tags Git**: Para marcar las versiones
- **GitHub Releases**: Para documentar los cambios

## 🔄 Tipos de Versionado

### 1. Versionado Automático
Se ejecuta automáticamente en cada push a la rama `main` y analiza los commits para determinar qué tipo de versión generar.

### 2. Versionado Manual
Permite crear versiones manualmente a través de GitHub Actions con control total sobre el tipo de versión.

## 📝 Conventional Commits

El sistema utiliza Conventional Commits para determinar automáticamente el tipo de versión:

### Formato de Commits

```
<tipo>[ámbito opcional]: <descripción>

[cuerpo opcional]

[pie opcional]
```

### Tipos de Commits y Versionado

| Tipo de Commit | Incremento de Versión | Ejemplo |
|---|---|---|
| `feat:` o `feature:` | **Minor** (v1.1.0 → v1.2.0) | `feat: añadir autenticación de usuarios` |
| `feat!:` o `BREAKING CHANGE:` | **Major** (v1.1.0 → v2.0.0) | `feat!: cambiar API de autenticación` |
| `fix:` | **Patch** (v1.1.0 → v1.1.1) | `fix: corregir error en cálculo de gastos` |
| `perf:` | **Patch** | `perf: optimizar consultas de base de datos` |
| `refactor:` | **Patch** | `refactor: reorganizar componentes Blazor` |
| Otros | **No genera versión** | `docs:`, `style:`, `test:`, etc. |

### Ejemplos de Commits

```bash
# Incrementa versión minor (nueva funcionalidad)
git commit -m "feat: agregar filtro por categorías en gastos"

# Incrementa versión patch (corrección de bug)
git commit -m "fix: corregir formato de fecha en listado"

# Incrementa versión major (cambio que rompe compatibilidad)
git commit -m "feat!: cambiar estructura de respuesta de API"

# No genera nueva versión
git commit -m "docs: actualizar documentación de instalación"
```

## ⚙️ Workflows de GitHub Actions

### 1. Auto Version & Tag (`.github/workflows/auto-version.yml`)

**Cuándo se ejecuta:**
- Automáticamente en cada push a `main`
- Manualmente desde GitHub Actions

**Proceso:**
1. **Análisis de Commits**: Examina commits desde la última tag
2. **Determinación de Versión**: Aplica Conventional Commits
3. **Creación de Tag**: Genera y pushea la nueva tag
4. **Creación de Release**: Genera release con changelog automático

**Lógica de Decisión:**
```bash
# Si encuentra commits con feat!: o BREAKING CHANGE:
Major version (v1.0.0 → v2.0.0)

# Si encuentra commits con feat: o feature:
Minor version (v1.1.0 → v1.2.0)

# Si encuentra commits con fix:, perf:, refactor:
Patch version (v1.1.1 → v1.1.2)

# Si no encuentra conventional commits
No genera versión
```

### 2. Manual Version & Tag (`.github/workflows/manual-version.yml`)

**Cuándo se ejecuta:**
- Solo manualmente desde GitHub Actions

**Opciones disponibles:**
- **Tipo de versión**: `patch`, `minor`, `major`
- **Versión personalizada**: Formato `v1.2.3` (opcional)

**Proceso:**
1. **Cálculo de Versión**: Basado en selección del usuario
2. **Validación**: Verifica que la tag no exista
3. **Creación de Tag**: Genera y pushea la nueva tag
4. **Creación de Release**: Genera release con commits recientes

## 📊 Modelo de Versión

### Formato de Versión
```
v<MAJOR>.<MINOR>.<PATCH>
```

### Incrementos
- **MAJOR** (v1.0.0 → v2.0.0): Cambios que rompen compatibilidad
- **MINOR** (v1.1.0 → v1.2.0): Nuevas funcionalidades compatibles
- **PATCH** (v1.1.1 → v1.1.2): Correcciones de bugs

### Ejemplos de Evolución
```
v0.1.0  ← Versión inicial
v0.1.1  ← Fix: corrección de bug
v0.2.0  ← Feat: nueva funcionalidad
v0.2.1  ← Fix: corrección menor
v1.0.0  ← Feat!: primera versión estable
v1.0.1  ← Fix: corrección post-release
v1.1.0  ← Feat: nueva funcionalidad
```

## 🔐 Configuración y Permisos

### Permisos Requeridos en GitHub

Los workflows requieren estos permisos en `permissions:`:

```yaml
permissions:
  contents: write    # Crear tags y pushear
  actions: read      # Leer información del workflow
```

### Configuración del Repositorio

En **Settings → Actions → General**:
- ✅ **Workflow permissions**: "Read and write permissions"
- ✅ **Allow GitHub Actions to create and approve pull requests**

### Variables de Entorno

Los workflows utilizan:
- `GITHUB_TOKEN`: Token automático de GitHub (no requiere configuración)
- `github.repository`: Variable automática con el nombre del repositorio

## 🚀 Uso Práctico

### Desarrollo Normal (Versionado Automático)

1. **Desarrollar funcionalidad:**
   ```bash
   # Trabajar en feature branch
   git checkout -b feature/nueva-funcionalidad
   # ... realizar cambios ...
   git commit -m "feat: agregar dashboard de análisis de gastos"
   ```

2. **Merge a main:**
   ```bash
   git checkout main
   git merge feature/nueva-funcionalidad
   git push origin main
   ```

3. **Resultado:** El workflow automático creará `v1.2.0` (nueva funcionalidad)

### Versionado Manual

1. **Ir a GitHub Actions** en el repositorio
2. **Seleccionar "Manual Version & Tag"**
3. **Click en "Run workflow"**
4. **Elegir opciones:**
   - **Branch**: `main`
   - **Version bump type**: `patch`, `minor`, o `major`
   - **Custom version** (opcional): `v2.0.0`
5. **Click "Run workflow"**

### Corrección Rápida (Hotfix)

```bash
# Fix urgente
git commit -m "fix: corregir error crítico en cálculo de totales"
git push origin main
# → Genera automáticamente v1.1.1
```

## 🔧 Integración con la Aplicación

### Modelo de Versión en Blazor

La aplicación incluye un modelo `VersionInfo` que puede ser utilizado para mostrar información de versión:

```csharp
// Gastos.Pwa/Models/VersionInfo.cs
public class VersionInfo
{
    public string Version { get; set; } = "1.0.0";
    public string Build { get; set; } = "0";
    public string Commit { get; set; } = "unknown";
    public string Branch { get; set; } = "unknown";
    public string Date { get; set; } = "";
    public string BuildDate { get; set; } = "";
    
    public string FullVersion => $"{Version}.{Build}";
    public string ShortCommit => Commit.Length > 7 ? Commit[..7] : Commit;
    public string DisplayText => $"v{FullVersion} ({ShortCommit})";
}
```

### Mostrar Versión en la UI

```razor
@* Ejemplo de uso en un componente Blazor *@
<div class="version-info">
    <span>Versión: @versionInfo.DisplayText</span>
    <span>Build: @versionInfo.FormattedBuildDate</span>
</div>
```

### Integración con CI/CD

El sistema de versionado puede integrarse fácilmente con:
- **Azure DevOps**: Usar las tags para despliegues
- **Docker**: Usar versiones como tags de imagen
- **NuGet**: Versionar paquetes automáticamente

## 📈 Beneficios del Sistema

### ✅ Ventajas

- **Automatización completa**: Sin intervención manual en desarrollo normal
- **Consistencia**: Siempre sigue las mismas reglas
- **Trazabilidad**: Cada versión está ligada a commits específicos
- **Flexibilidad**: Permite versionado manual cuando es necesario
- **Documentación**: Releases automáticos con changelog
- **Estándar**: Sigue Conventional Commits y SemVer

### 🎯 Casos de Uso

- **Desarrollo continuo**: Versiones automáticas en cada feature
- **Releases planificados**: Control manual para versiones importantes
- **Hotfixes**: Versiones de corrección rápidas y automáticas
- **Branches de release**: Preparación de versiones específicas

## 🔍 Solución de Problemas

### Problema: El workflow no se ejecuta
**Solución:** Verificar permisos en Settings → Actions → General

### Problema: Error 403 al crear tag
**Solución:** Verificar que `contents: write` esté en permissions

### Problema: No se genera versión automática
**Solución:** Verificar que los commits sigan Conventional Commits

### Problema: Tag ya existe
**Solución:** El workflow automático detecta esto y no genera duplicados

## 📚 Referencias

- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github)

---

**Última actualización:** Este documento describe el sistema implementado en GastosApp v1.0.0