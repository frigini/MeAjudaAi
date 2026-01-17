namespace MeAjudaAi.Web.Admin.Services.Resilience.Http;

/// <summary>
/// Status da conexão com a API.
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// Conectado e funcionando normalmente.
    /// </summary>
    Connected,
    
    /// <summary>
    /// Tentando reconectar (circuit breaker half-open ou retrying).
    /// </summary>
    Reconnecting,
    
    /// <summary>
    /// Desconectado (circuit breaker open).
    /// </summary>
    Disconnected
}
