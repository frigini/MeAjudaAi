namespace MeAjudaAi.Web.Admin.Services.Resilience;

/// <summary>
/// Status da conexão com a API
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// Conectado e funcionando normalmente
    /// </summary>
    Connected,
    
    /// <summary>
    /// Tentando reconectar (circuit breaker half-open ou retrying)
    /// </summary>
    Reconnecting,
    
    /// <summary>
    /// Desconectado (circuit breaker open)
    /// </summary>
    Disconnected
}

/// <summary>
/// Serviço para rastrear o status da conexão com a API
/// </summary>
public interface IConnectionStatusService
{
    /// <summary>
    /// Status atual da conexão
    /// </summary>
    ConnectionStatus CurrentStatus { get; }
    
    /// <summary>
    /// Evento disparado quando o status muda
    /// </summary>
    event EventHandler<ConnectionStatus>? StatusChanged;
    
    /// <summary>
    /// Atualiza o status da conexão
    /// </summary>
    void UpdateStatus(ConnectionStatus status);
}

/// <summary>
/// Implementação do serviço de status de conexão
/// </summary>
public class ConnectionStatusService : IConnectionStatusService
{
    private ConnectionStatus _currentStatus = ConnectionStatus.Connected;

    public ConnectionStatus CurrentStatus => _currentStatus;

    public event EventHandler<ConnectionStatus>? StatusChanged;

    public void UpdateStatus(ConnectionStatus status)
    {
        if (_currentStatus != status)
        {
            _currentStatus = status;
            StatusChanged?.Invoke(this, status);
        }
    }
}
