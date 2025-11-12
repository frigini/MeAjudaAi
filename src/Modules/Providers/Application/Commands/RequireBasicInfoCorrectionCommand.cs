using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para retornar um prestador de serviços para correção de informações básicas
/// durante o processo de verificação de documentos.
/// </summary>
/// <param name="ProviderId">Identificador do prestador de serviços</param>
/// <param name="Reason">Motivo da correção necessária (obrigatório para auditoria e notificação)</param>
/// <param name="RequestedBy">Quem está solicitando a correção (verificador/administrador)</param>
public sealed record RequireBasicInfoCorrectionCommand(
    Guid ProviderId,
    string Reason,
    string RequestedBy
) : Command<Result>;
