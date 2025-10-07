using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando o nome de usuário (username) é alterado.
/// </summary>
/// <remarks>
/// Este evento é publicado quando o username de um usuário é atualizado através do método ChangeUsername.
/// Pode ser usado para sincronização com sistemas externos (como Keycloak),
/// validação de unicidade de username, trilhas de auditoria, serviços de notificação, etc.
/// Importante: Mudanças de username podem afetar a autenticação e devem ser tratadas com cuidado.
/// </remarks>
/// <param name="AggregateId">Identificador único do usuário cujo username foi alterado</param>
/// <param name="Version">Versão do agregado quando o evento ocorreu</param>
/// <param name="OldUsername">Username anterior</param>
/// <param name="NewUsername">Novo username</param>
public record UserUsernameChangedEvent(
    Guid AggregateId,
    int Version,
    Username OldUsername,
    Username NewUsername
) : DomainEvent(AggregateId, Version);