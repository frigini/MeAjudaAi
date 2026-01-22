using MeAjudaAi.Contracts.Configuration;

namespace MeAjudaAi.Web.Admin.Services.Interfaces;

/// <summary>
/// Serviço para buscar configuração do backend ao iniciar o aplicativo.
/// Permite configuração dinâmica sem expor informações sensíveis no wwwroot.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Obtém a configuração do cliente a partir do backend.
    /// </summary>
    Task<ClientConfiguration> GetClientConfigurationAsync(CancellationToken cancellationToken = default);
}
