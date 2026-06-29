using MeAjudaAi.Modules.Users.Domain.Events;
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
public class UserDeletedDomainEventHandlerTests : BaseInMemoryDatabaseTest<UsersDbContext>
{
    private readonly Mock<IMessageBus> _messageBusMock;

    public UserDeletedDomainEventHandlerTests() : base(options => new UsersDbContext(options, null!))
    {
        _messageBusMock = new Mock<IMessageBus>();
    }

    private UserDeletedDomainEventHandler CreateHandler() =>
        new(_messageBusMock.Object, DbContext, NullLogger<UserDeletedDomainEventHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var user = new UserBuilder()
            .WithId(userId)
            .WithEmail("john@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<UserDeletedIntegrationEvent>(e =>
                    e.UserId == userId &&
                    e.Email == "john@example.com" &&
                    e.FirstName == "John"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ShouldPublishWithFallbackData()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<UserDeletedIntegrationEvent>(e =>
                    e.UserId == userId &&
                    e.Email == "desconhecido" &&
                    e.FirstName == "Usuário"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusThrows_ShouldPropagateException()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var user = new UserBuilder()
            .WithId(userId)
            .WithEmail("john@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<UserDeletedIntegrationEvent>(),
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

    [Theory]
    [InlineData("ffffffff-ffff-ffff-ffff-ffffffffffff")]
    public async Task HandleAsync_WithEdgeCaseUserIds_ShouldPublishEvent(string userIdString)
    {
        // Arrange
        var userId = Guid.Parse(userIdString);
        var user = new UserBuilder()
            .WithId(userId)
            .WithEmail("edge@example.com")
            .WithFirstName("Edge")
            .WithLastName("Case")
            .Build();

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<UserDeletedIntegrationEvent>(e => e.UserId == userId),
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
        var userId = UuidGenerator.NewId();
        var user = new UserBuilder()
            .WithId(userId)
            .WithEmail("john@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, token);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserDeletedIntegrationEvent>(),
                It.IsAny<string>(),
                It.Is<CancellationToken>(ct => ct == token)
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_MultipleEvents_ShouldPublishAll()
    {
        // Arrange
        var userId1 = UuidGenerator.NewId();
        var userId2 = UuidGenerator.NewId();
        var userId3 = UuidGenerator.NewId();

        var users = new[]
        {
            new UserBuilder().WithId(userId1).WithEmail("u1@test.com").WithFirstName("U1").Build(),
            new UserBuilder().WithId(userId2).WithEmail("u2@test.com").WithFirstName("U2").Build(),
            new UserBuilder().WithId(userId3).WithEmail("u3@test.com").WithFirstName("U3").Build()
        };

        await DbContext.Users.AddRangeAsync(users);
        await DbContext.SaveChangesAsync();

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(new UserDeletedDomainEvent(userId1, 1), CancellationToken.None);
        await handler.HandleAsync(new UserDeletedDomainEvent(userId2, 1), CancellationToken.None);
        await handler.HandleAsync(new UserDeletedDomainEvent(userId3, 1), CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserDeletedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Exactly(3)
        );
    }
}
