using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para rejeitar o registro de um prestador de serviços.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços</param>
/// <param name="RejectedBy">Quem está executando a rejeição</param>
/// <param name="Reason">Motivo da rejeição (opcional)</param>
public sealed record RejectProviderCommand(
    Guid ProviderId,
    string? RejectedBy = null,
    string? Reason = null
) : Command<Result>;
