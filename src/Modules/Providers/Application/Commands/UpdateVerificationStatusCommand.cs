using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para atualização do status de verificação do prestador de serviços.
/// </summary>
public sealed record UpdateVerificationStatusCommand(
    Guid ProviderId,
    EVerificationStatus Status,
    string? UpdatedBy = null
) : Command<Result<ProviderDto>>;