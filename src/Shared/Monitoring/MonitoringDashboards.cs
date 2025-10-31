namespace MeAjudaAi.Shared.Monitoring;

/// <summary>
/// Classe de configuração para dashboards customizados
/// </summary>
public static class MonitoringDashboards
{
    /// <summary>
    /// Configuração de dashboard para métricas de negócio
    /// </summary>
    internal static class BusinessDashboard
    {
        public const string DashboardName = "MeAjudaAi Business Metrics";

        public static readonly string[] KeyMetrics = new[]
        {
            "meajudaai.users.registrations.total",
            "meajudaai.users.logins.total",
            "meajudaai.users.active.current",
            "meajudaai.help_requests.created.total",
            "meajudaai.help_requests.completed.total",
            "meajudaai.help_requests.pending.current",
            "meajudaai.help_requests.duration.seconds"
        };

        public static readonly string[] AlertRules = new[]
        {
            "meajudaai.help_requests.pending.current > 100",
            "meajudaai.help_requests.duration.seconds > 3600", // 1 hora
            "rate(meajudaai.api.calls.total[5m]) > 1000" // Mais de 1000 calls por minuto
        };
    }

    /// <summary>
    /// Configuração de dashboard para performance
    /// </summary>
    internal static class PerformanceDashboard
    {
        public const string DashboardName = "MeAjudaAi Performance";

        public static readonly string[] KeyMetrics = new[]
        {
            "http_request_duration_seconds",
            "meajudaai.database.query.duration.seconds",
            "process_working_set_bytes",
            "dotnet_gc_collection_count",
            "aspnetcore_requests_per_second"
        };
    }
}
