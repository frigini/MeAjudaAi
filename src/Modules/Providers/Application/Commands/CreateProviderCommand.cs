using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para criação de um novo prestador de serviços no sistema.
/// </summary>
public sealed record CreateProviderCommand(
    Guid UserId,
    string Name,
    EProviderType Type,
    BusinessProfileDto BusinessProfile
) : Command<Result<ProviderDto>>;
