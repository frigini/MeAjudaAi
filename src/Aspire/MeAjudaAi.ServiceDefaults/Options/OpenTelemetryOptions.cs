namespace MeAjudaAi.ServiceDefaults.Options;

/// <summary>
/// Configurações para OpenTelemetry
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// URL do endpoint OTLP
    /// </summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Nome do serviço para telemetria
    /// </summary>
    public string ServiceName { get; set; } = "MeAjudaAi";

    /// <summary>
    /// Versão do serviço
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Indica se a exportação está habilitada
    /// </summary>
    public bool ExportEnabled { get; set; } = true;

    /// <summary>
    /// Indica se deve exportar para console (usado em desenvolvimento)
    /// </summary>
    public bool ExportToConsole { get; set; } = false;
}
