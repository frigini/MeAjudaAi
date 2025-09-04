using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;

public sealed class UserRegisteredDomainEventHandler : IEventHandler<UserRegisteredDomainEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly UsersDbContext _context;
    private readonly ILogger<UserRegisteredDomainEventHandler> _logger;

    public UserRegisteredDomainEventHandler(
        IMessageBus messageBus,
        UsersDbContext context,
        ILogger<UserRegisteredDomainEventHandler> logger)
    {
        _messageBus = messageBus;
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the full user data from database to ensure we have all information
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == domainEvent.AggregateId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User not found for UserRegisteredDomainEvent: {UserId}", domainEvent.AggregateId);
                return;
            }

            var integrationEvent = new Shared.Messaging.Messages.Users.IntegrationEvent(
                user.Id.Value,
                user.Email.Value,
                user.Profile.FirstName,
                user.Profile.LastName,
                user.KeycloakId,
                user.Roles.ToList(),
                user.CreatedAt
            );

            await _messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            _logger.LogInformation("Published UserRegistered integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserRegisteredDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw;
        }
    }
}

public sealed class UserProfileUpdatedDomainEventHandler : IEventHandler<UserProfileUpdatedDomainEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly UsersDbContext _context;
    private readonly ILogger<UserProfileUpdatedDomainEventHandler> _logger;

    public UserProfileUpdatedDomainEventHandler(
        IMessageBus messageBus,
        UsersDbContext context,
        ILogger<UserProfileUpdatedDomainEventHandler> logger)
    {
        _messageBus = messageBus;
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(UserProfileUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == domainEvent.AggregateId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User not found for UserProfileUpdatedDomainEvent: {UserId}", domainEvent.AggregateId);
                return;
            }

            var integrationEvent = new UserProfileUpdatedIntegrationEvent(
                user.Id.Value,
                user.Email.Value,
                user.Profile.FirstName,
                user.Profile.LastName,
                user.UpdatedAt ?? domainEvent.OccurredAt
            );

            await _messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            _logger.LogInformation("Published UserProfileUpdated integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserProfileUpdatedDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw;
        }
    }
}

public sealed class UserDeactivatedDomainEventHandler : IEventHandler<UserDeactivatedDomainEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly UsersDbContext _context;
    private readonly ILogger<UserDeactivatedDomainEventHandler> _logger;

    public UserDeactivatedDomainEventHandler(
        IMessageBus messageBus,
        UsersDbContext context,
        ILogger<UserDeactivatedDomainEventHandler> logger)
    {
        _messageBus = messageBus;
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(UserDeactivatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == domainEvent.AggregateId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User not found for UserDeactivatedDomainEvent: {UserId}", domainEvent.AggregateId);
                return;
            }

            var integrationEvent = new UserDeactivatedIntegrationEvent(
                user.Id.Value,
                user.Email.Value,
                domainEvent.Reason,
                domainEvent.OccurredAt
            );

            await _messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            _logger.LogInformation("Published UserDeactivated integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserDeactivatedDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw;
        }
    }
}

public sealed class UserRoleChangedDomainEventHandler : IEventHandler<UserRoleAssignedDomainEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly UsersDbContext _context;
    private readonly ILogger<UserRoleChangedDomainEventHandler> _logger;

    public UserRoleChangedDomainEventHandler(
        IMessageBus messageBus,
        UsersDbContext context,
        ILogger<UserRoleChangedDomainEventHandler> logger)
    {
        _messageBus = messageBus;
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(UserRoleAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id.Value == domainEvent.AggregateId, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("User not found for UserRoleChangedDomainEvent: {UserId}", domainEvent.AggregateId);
                return;
            }

            var integrationEvent = new UserRoleChangedIntegrationEvent(
                user.Id.Value,
                user.Email.Value,
                domainEvent.PreviousRoles,
                domainEvent.NewRole,
                domainEvent.ChangedBy,
                domainEvent.OccurredAt
            );

            await _messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            _logger.LogInformation("Published UserRoleChanged integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserRoleChangedDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw;
        }
    }
}

public sealed class UserTierChangedDomainEventHandler : IEventHandler<UserTierChangedDomainEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly UsersDbContext _context;
    private readonly ILogger<UserTierChangedDomainEventHandler> _logger;

    public UserTierChangedDomainEventHandler(
        IMessageBus messageBus,
        UsersDbContext context,
        ILogger<UserTierChangedDomainEventHandler> logger)
    {
        _messageBus = messageBus;
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(UserTierChangedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get both user and service provider data
            var user = await _context.Users
                .Include(u => u.ServiceProvider)
                .FirstOrDefaultAsync(u => u.Id.Value == domainEvent.UserId, cancellationToken);

            if (user?.ServiceProvider is null)
            {
                _logger.LogWarning("User or ServiceProvider not found for UserTierChangedDomainEvent: {UserId}", domainEvent.UserId);
                return;
            }

            var integrationEvent = new ServiceProviderTierChanged(
                user.Id.Value,
                user.ServiceProvider.Id.Value,
                user.ServiceProvider.CompanyName,
                domainEvent.PreviousTier,
                domainEvent.NewTier,
                domainEvent.ChangedBy,
                domainEvent.ChangedAt
            );

            await _messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            _logger.LogInformation("Published ServiceProviderTierChanged integration event for user {UserId}", domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserTierChangedDomainEvent for user {UserId}", domainEvent.UserId);
            throw;
        }
    }
}

public sealed class UserSubscriptionUpdatedDomainEventHandler : IEventHandler<UserSubscriptionUpdatedDomainEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly UsersDbContext _context;
    private readonly ILogger<UserSubscriptionUpdatedDomainEventHandler> _logger;

    public UserSubscriptionUpdatedDomainEventHandler(
        IMessageBus messageBus,
        UsersDbContext context,
        ILogger<UserSubscriptionUpdatedDomainEventHandler> logger)
    {
        _messageBus = messageBus;
        _context = context;
        _logger = logger;
    }

    public async Task HandleAsync(UserSubscriptionUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.ServiceProvider)
                .FirstOrDefaultAsync(u => u.Id.Value == domainEvent.UserId, cancellationToken);

            if (user?.ServiceProvider is null)
            {
                _logger.LogWarning("User or ServiceProvider not found for UserSubscriptionUpdatedDomainEvent: {UserId}", domainEvent.UserId);
                return;
            }

            var integrationEvent = new ServiceProviderSubscriptionUpdated(
                user.Id.Value,
                user.ServiceProvider.Id.Value,
                domainEvent.SubscriptionId,
                domainEvent.Status,
                domainEvent.ExpiresAt,
                domainEvent.UpdatedAt
            );

            await _messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            _logger.LogInformation("Published ServiceProviderSubscriptionUpdated integration event for user {UserId}", domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle UserSubscriptionUpdatedDomainEvent for user {UserId}", domainEvent.UserId);
            throw;
        }
    }
}