namespace MeAjudaAi.ServiceDefaults.Options;

public sealed class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";
    public ExporterOptions Exporters { get; set; } = new();
}

public sealed class ExporterOptions
{
    public bool OtlpEnabled { get; set; } = true;
    public bool ConsoleEnabled { get; set; } = false;
    public bool PrometheusEnabled { get; set; } = false;
}