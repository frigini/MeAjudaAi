using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Evento de domínio disparado quando um novo usuário é registrado no sistema.
/// </summary>
/// <remarks>
/// Este evento é publicado automaticamente quando um usuário é criado através do construtor
/// da entidade User. Pode ser usado para integração com outros bounded contexts,
/// envio de emails de boas-vindas, criação de perfis em outros sistemas, etc.
/// </remarks>
/// <param name="AggregateId">Identificador único do usuário registrado</param>
/// <param name="Version">Versão do agregado no momento do evento</param>
/// <param name="Email">Endereço de email do usuário registrado</param>
/// <param name="Username">Nome de usuário escolhido</param>
/// <param name="FirstName">Primeiro nome do usuário</param>
/// <param name="LastName">Sobrenome do usuário</param>
public record UserRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    string Email,
    Username Username,
    string FirstName,
    string LastName
) : DomainEvent(AggregateId, Version);
