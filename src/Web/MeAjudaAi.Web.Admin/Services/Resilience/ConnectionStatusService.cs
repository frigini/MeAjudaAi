namespace MeAjudaAi.Web.Admin.Services.Resilience;

/// <summary>
/// Implementação do serviço de status de conexão.
/// Monitora e notifica mudanças no estado de conectividade com a API.
/// </summary>
public class ConnectionStatusService : IConnectionStatusService
{
    private ConnectionStatus _currentStatus = ConnectionStatus.Connected;

    /// <inheritdoc />
    public ConnectionStatus CurrentStatus => _currentStatus;

    /// <inheritdoc />
    public event EventHandler<ConnectionStatus>? StatusChanged;

    /// <inheritdoc />
    public void UpdateStatus(ConnectionStatus status)
    {
        if (_currentStatus != status)
        {
            _currentStatus = status;
            StatusChanged?.Invoke(this, status);
        }
    }
}
