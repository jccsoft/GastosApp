using System.Net.Http.Json;
using System.Reflection;
using Gastos.Pwa.Models;

namespace Gastos.Pwa.Services;

public interface IVersionService
{
    Task<VersionInfo> GetVersionInfoAsync();
    VersionInfo GetAssemblyVersionInfo();
}

public class VersionService : IVersionService
{
    private readonly HttpClient _httpClient;
    private VersionInfo? _cachedVersionInfo;

    public VersionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<VersionInfo> GetVersionInfoAsync()
    {
        if (_cachedVersionInfo != null)
        {
            return _cachedVersionInfo;
        }

        try
        {
            // Intentar obtener la información de versión del archivo JSON generado en build
            _cachedVersionInfo = await _httpClient.GetFromJsonAsync<VersionInfo>("version-info.json");
            
            if (_cachedVersionInfo != null)
            {
                return _cachedVersionInfo;
            }
        }
        catch
        {
            // Si no se puede obtener del archivo, usar la información del assembly
        }

        // Fallback a información del assembly
        _cachedVersionInfo = GetAssemblyVersionInfo();
        return _cachedVersionInfo;
    }

    public VersionInfo GetAssemblyVersionInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName();
        
        var versionInfo = new VersionInfo
        {
            Version = assemblyName.Version?.ToString(3) ?? "1.0.0",
            Build = assemblyName.Version?.Revision.ToString() ?? "0",
            Commit = GetInformationalVersion(assembly),
            Branch = "unknown",
            Date = GetBuildDate(assembly),
            BuildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        return versionInfo;
    }

    private static string GetInformationalVersion(Assembly assembly)
    {
        var infoVersionAttr = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (infoVersionAttr?.InformationalVersion != null)
        {
            // Extraer el hash del commit de la versión informacional
            var parts = infoVersionAttr.InformationalVersion.Split('-');
            if (parts.Length > 2)
            {
                return parts[^1]; // Último elemento (hash del commit)
            }
        }
        return "unknown";
    }

    private static string GetBuildDate(Assembly assembly)
    {
        try
        {
            // Intentar obtener la fecha de build del assembly
            var buildDate = new FileInfo(assembly.Location).LastWriteTime;
            return buildDate.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}