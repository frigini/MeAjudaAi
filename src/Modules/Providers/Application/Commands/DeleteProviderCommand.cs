using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para exclusão lógica do prestador de serviços.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DeleteProviderCommand(
    Guid ProviderId,
    string? DeletedBy = null
) : Command<Result>;
