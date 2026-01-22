using MeAjudaAi.Web.Admin.Services.Resilience.Interfaces;

namespace MeAjudaAi.Web.Admin.Services.Resilience.Http;

/// <summary>
/// Implementação do serviço de status de conexão.
/// Monitora e notifica mudanças no estado de conectividade com a API.
/// </summary>
public class ConnectionStatusService : IConnectionStatusService
{
    private readonly object _lock = new();
    private ConnectionStatus _currentStatus = ConnectionStatus.Connected;

    /// <inheritdoc />
    public ConnectionStatus CurrentStatus => _currentStatus;

    /// <inheritdoc />
    public event EventHandler<ConnectionStatus>? StatusChanged;

    /// <inheritdoc />
    public void UpdateStatus(ConnectionStatus status)
    {
        lock (_lock)
        {
            if (_currentStatus != status)
            {
                _currentStatus = status;
                StatusChanged?.Invoke(this, status);
            }
        }
    }
}
