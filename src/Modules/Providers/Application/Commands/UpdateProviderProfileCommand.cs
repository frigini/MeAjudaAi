using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para atualização do perfil do prestador de serviços.
/// </summary>
public sealed record UpdateProviderProfileCommand(
    Guid ProviderId,
    string Name,
    BusinessProfileDto BusinessProfile,
    string? UpdatedBy = null
) : Command<Result<ProviderDto>>;
