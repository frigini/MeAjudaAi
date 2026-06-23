using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para desativar o perfil do prestador de serviços.
/// </summary>
public sealed record DeactivateProviderProfileCommand(
    Guid ProviderId,
    string? UpdatedBy = null
) : Command<Result>;