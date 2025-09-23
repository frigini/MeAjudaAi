using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando o endereço de email de um usuário é alterado.
/// </summary>
/// <remarks>
/// Este evento é publicado quando o email de um usuário é atualizado através do método ChangeEmail.
/// Pode ser usado para sincronização com sistemas externos (como Keycloak),
/// fluxos de verificação de email, serviços de notificação, etc.
/// Importante: Mudanças de email podem requerer re-autenticação em alguns sistemas.
/// </remarks>
/// <param name="AggregateId">Identificador único do usuário cujo email foi alterado</param>
/// <param name="Version">Versão do agregado quando o evento ocorreu</param>
/// <param name="OldEmail">Endereço de email anterior</param>
/// <param name="NewEmail">Novo endereço de email</param>
public record UserEmailChangedEvent(
    Guid AggregateId,
    int Version,
    string OldEmail,
    string NewEmail
) : DomainEvent(AggregateId, Version);