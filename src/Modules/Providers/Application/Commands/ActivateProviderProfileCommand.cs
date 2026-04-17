using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para reativar o perfil do prestador de serviços.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ActivateProviderProfileCommand(
    Guid ProviderId,
    string? UpdatedBy = null
) : Command<Result>;
