using System.Text.Json.Serialization;

namespace Gastos.Pwa.Models;

public class VersionInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("build")]
    public string Build { get; set; } = "0";

    [JsonPropertyName("commit")]
    public string Commit { get; set; } = "unknown";

    [JsonPropertyName("branch")]
    public string Branch { get; set; } = "unknown";

    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("buildDate")]
    public string BuildDate { get; set; } = "";

    /// <summary>
    /// Obtiene la versión completa en formato semántico
    /// </summary>
    public string FullVersion => $"{Version}.{Build}";

    /// <summary>
    /// Obtiene el commit hash corto para mostrar
    /// </summary>
    public string ShortCommit => Commit.Length > 7 ? Commit[..7] : Commit;

    /// <summary>
    /// Obtiene la fecha de commit formateada
    /// </summary>
    public string FormattedCommitDate
    {
        get
        {
            if (DateTime.TryParse(Date, out var commitDate))
            {
                return commitDate.ToString("dd/MM/yyyy HH:mm");
            }
            return Date;
        }
    }

    /// <summary>
    /// Obtiene la fecha de build formateada
    /// </summary>
    public string FormattedBuildDate
    {
        get
        {
            if (DateTime.TryParse(BuildDate, out var buildDate))
            {
                return buildDate.ToString("dd/MM/yyyy HH:mm");
            }
            return BuildDate;
        }
    }

    /// <summary>
    /// Obtiene un texto descriptivo de la versión
    /// </summary>
    public string DisplayText => $"v{FullVersion} ({ShortCommit})";
}