using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para suspender um prestador de serviços.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços</param>
/// <param name="SuspendedBy">Quem está executando a suspensão</param>
/// <param name="Reason">Motivo da suspensão (opcional)</param>
public sealed record SuspendProviderCommand(
    Guid ProviderId,
    string? SuspendedBy = null,
    string? Reason = null
) : Command<Result>;
