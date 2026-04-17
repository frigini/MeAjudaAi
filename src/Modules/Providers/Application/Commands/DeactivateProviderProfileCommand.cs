using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para desativar o perfil do prestador de serviços.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DeactivateProviderProfileCommand(
    Guid ProviderId,
    string? UpdatedBy = null
) : Command<Result>;
