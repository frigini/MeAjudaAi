using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class UserRegisteredDomainEventHandlerTests : BaseInMemoryDatabaseTest<UsersDbContext>
{
    private readonly Mock<IMessageBus> _messageBusMock;

    public UserRegisteredDomainEventHandlerTests() : base(options => new UsersDbContext(options, null!))
    {
        _messageBusMock = new Mock<IMessageBus>();
    }

    private UserRegisteredDomainEventHandler CreateHandler() =>
        new(_messageBusMock.Object, DbContext, NullLogger<UserRegisteredDomainEventHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WithValidUser_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .WithFirstName("Test")
            .WithLastName("User")
            .Build();

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserRegisteredDomainEvent(
            user.Id.Value,
            1,
            user.Email.Value,
            user.Username,
            "Test",
            "User"
        );

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<UserRegisteredIntegrationEvent>(e =>
                    e.UserId == user.Id.Value &&
                    e.Email == user.Email.Value &&
                    e.FirstName == "Test" &&
                    e.LastName == "User" &&
                    e.Roles.Contains("customer")
                ),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldLogWarningAndNotPublish()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var domainEvent = new UserRegisteredDomainEvent(
            userId,
            1,
            "nonexistent@example.com",
            new Username("nonexistent"),
            "Non",
            "Existent"
        );

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserRegisteredIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusThrows_ShouldPropagateException()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserRegisteredDomainEvent(
            user.Id.Value,
            1,
            user.Email.Value,
            user.Username,
            "Test",
            "User"
        );

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<UserRegisteredIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ThrowsAsync(new InvalidOperationException("Message bus unavailable"));

        // Act & Assert
        var handler = CreateHandler();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(domainEvent, CancellationToken.None)
        );
    }

    [Fact]
    public async Task HandleAsync_WithUserWithKeycloakId_ShouldIncludeKeycloakId()
    {
        var keycloakId = Guid.NewGuid().ToString();
        var user = new UserBuilder()
            .WithKeycloakId(keycloakId)
            .Build();

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserRegisteredDomainEvent(
            user.Id.Value,
            1,
            user.Email.Value,
            user.Username,
            "Test",
            "User"
        );

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<UserRegisteredIntegrationEvent>(e =>
                    e.KeycloakId == keycloakId
                ),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToMessageBus()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserRegisteredDomainEvent(
            user.Id.Value,
            1,
            user.Email.Value,
            user.Username,
            "Test",
            "User"
        );

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, cancellationToken);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserRegisteredIntegrationEvent>(),
                It.IsAny<string>(),
                cancellationToken
            ),
            Times.Once
        );
    }
}
