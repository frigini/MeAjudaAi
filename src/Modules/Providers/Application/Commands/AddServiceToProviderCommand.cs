using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para adicionar um serviço do catálogo a um provider.
/// </summary>
/// <param name="ProviderId">ID do provider</param>
/// <param name="ServiceId">ID do serviço do catálogo (módulo ServiceCatalogs)</param>
[ExcludeFromCodeCoverage]
public sealed record AddServiceToProviderCommand(
    Guid ProviderId,
    Guid ServiceId
) : Command<Result>;
