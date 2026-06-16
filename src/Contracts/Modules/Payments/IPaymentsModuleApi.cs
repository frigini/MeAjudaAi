using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Payments.DTOs;

namespace MeAjudaAi.Contracts.Modules.Payments;

/// <summary>
/// API pública do módulo Payments para consumo por outros módulos.
/// </summary>
public interface IPaymentsModuleApi : IModuleApi
{
    /// <summary>
    /// Obtém os detalhes de uma assinatura ativa para um prestador.
    /// </summary>
    /// <param name="providerId">ID do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da assinatura ou erro</returns>
    Task<Result<ModuleSubscriptionDto?>> GetActiveSubscriptionByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um prestador possui uma assinatura ativa.
    /// </summary>
    /// <param name="providerId">ID do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se possuir assinatura ativa, False caso contrário</returns>
    Task<Result<bool>> HasActiveSubscriptionAsync(Guid providerId, CancellationToken cancellationToken = default);
}
