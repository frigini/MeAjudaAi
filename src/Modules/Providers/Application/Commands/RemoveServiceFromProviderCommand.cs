using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para remover um serviço do catálogo de um provider.
/// </summary>
/// <param name="ProviderId">ID do provider</param>
/// <param name="ServiceId">ID do serviço do catálogo (módulo ServiceCatalogs)</param>
public sealed record RemoveServiceFromProviderCommand(
    Guid ProviderId,
    Guid ServiceId
) : Command<Result>;
