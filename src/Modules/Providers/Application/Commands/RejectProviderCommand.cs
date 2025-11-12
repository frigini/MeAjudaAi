using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para rejeitar o registro de um prestador de serviços.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços</param>
/// <param name="RejectedBy">Quem está executando a rejeição</param>
/// <param name="Reason">Motivo da rejeição (obrigatório para auditoria)</param>
public sealed record RejectProviderCommand(
    Guid ProviderId,
    string RejectedBy,
    string Reason
) : Command<Result>;
