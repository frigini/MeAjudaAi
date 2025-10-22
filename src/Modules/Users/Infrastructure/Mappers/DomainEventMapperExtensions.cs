using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;

namespace MeAjudaAi.Modules.Users.Infrastructure.Mappers;

/// <summary>
/// Métodos de extensão para mapeamento de Eventos de Domínio para Eventos de Integração
/// </summary>
public static class DomainEventMapperExtensions
{
    /// <summary>
    /// Mapeia UserRegisteredDomainEvent para UserRegisteredIntegrationEvent
    /// </summary>
    /// <param name="domainEvent">O evento de domínio a ser mapeado</param>
    /// <returns>Evento de integração para comunicação entre módulos</returns>
    public static UserRegisteredIntegrationEvent ToIntegrationEvent(this UserRegisteredDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new UserRegisteredIntegrationEvent(
            Source: "Users",
            UserId: domainEvent.AggregateId,
            Email: domainEvent.Email,
            Username: domainEvent.Username.Value,
            FirstName: domainEvent.FirstName,
            LastName: domainEvent.LastName,
            KeycloakId: string.Empty, // Será preenchido pela camada de infraestrutura
            Roles: [], // Será preenchido pela camada de infraestrutura
            RegisteredAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Mapeia UserProfileUpdatedDomainEvent para UserProfileUpdatedIntegrationEvent
    /// </summary>
    /// <param name="domainEvent">O evento de domínio a ser mapeado</param>
    /// <param name="email">O email do usuário (deve ser fornecido pelo repositório de usuários)</param>
    /// <returns>Evento de integração para comunicação entre módulos</returns>
    public static UserProfileUpdatedIntegrationEvent ToIntegrationEvent(this UserProfileUpdatedDomainEvent domainEvent, string email)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new UserProfileUpdatedIntegrationEvent(
            Source: "Users",
            UserId: domainEvent.AggregateId,
            Email: email,
            FirstName: domainEvent.FirstName,
            LastName: domainEvent.LastName,
            UpdatedAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Mapeia UserDeletedDomainEvent para UserDeletedIntegrationEvent
    /// </summary>
    /// <param name="domainEvent">O evento de domínio a ser mapeado</param>
    /// <returns>Evento de integração para comunicação entre módulos</returns>
    public static UserDeletedIntegrationEvent ToIntegrationEvent(this UserDeletedDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        return new UserDeletedIntegrationEvent(
            Source: "Users",
            UserId: domainEvent.AggregateId,
            DeletedAt: DateTime.UtcNow
        );
    }
}
