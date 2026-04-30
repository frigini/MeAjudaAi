namespace MeAjudaAi.Gateway.Resilience;

public class GatewayResilienceOptions
{
    public const string SectionName = "GatewayResilience";

    /// <summary>
    /// Tempo máximo em segundos para aguardar a resposta do upstream.
    /// Padrão: 30 segundos.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Número de retentativas em caso de falha transiente.
    /// Padrão: 3 retentativas com backoff exponencial.
    /// </summary>
    public int RetryCount { get; set; } = 3;
}