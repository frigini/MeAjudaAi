using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para o próprio prestador solicitar exclusão lógica de seu perfil.
/// </summary>
public sealed record DeleteMyProviderProfileCommand(
    Guid ProviderId,
    string? DeletedBy = null
) : Command<Result>;