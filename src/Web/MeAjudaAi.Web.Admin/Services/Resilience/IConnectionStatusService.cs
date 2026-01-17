namespace MeAjudaAi.Web.Admin.Services.Resilience;

/// <summary>
/// Serviço para rastrear o status da conexão com a API.
/// </summary>
public interface IConnectionStatusService
{
    /// <summary>
    /// Status atual da conexão.
    /// </summary>
    ConnectionStatus CurrentStatus { get; }
    
    /// <summary>
    /// Evento disparado quando o status muda.
    /// </summary>
    event EventHandler<ConnectionStatus>? StatusChanged;
    
    /// <summary>
    /// Atualiza o status da conexão.
    /// </summary>
    /// <param name="status">Novo status da conexão</param>
    void UpdateStatus(ConnectionStatus status);
}
