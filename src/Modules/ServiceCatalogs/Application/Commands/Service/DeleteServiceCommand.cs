using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para deletar um serviço do catálogo.
/// Nota: Melhoria futura necessária - implementar padrão de soft-delete (IsActive = false) para preservar
/// histórico de auditoria e prevenir deleção quando provedores referenciam este serviço. Veja TODO no handler.
/// </summary>
public sealed record DeleteServiceCommand(Guid Id) : Command<Result>;
