using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para atualização do perfil do prestador de serviços.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record UpdateProviderProfileCommand(
    Guid ProviderId,
    string Name,
    BusinessProfileDto BusinessProfile,
    List<ProviderServiceDto>? Services,
    string? UpdatedBy = null
) : Command<Result<ProviderDto>>;
