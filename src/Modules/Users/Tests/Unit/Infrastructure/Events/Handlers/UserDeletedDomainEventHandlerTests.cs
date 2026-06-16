using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Users;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class UserDeletedDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<ILogger<UserDeletedDomainEventHandler>> _loggerMock;
    private readonly UsersDbContext _context;
    private readonly UserDeletedDomainEventHandler _handler;

    public UserDeletedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        _loggerMock = new Mock<ILogger<UserDeletedDomainEventHandler>>();

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsersDbContext(options, null!);
        _handler = new UserDeletedDomainEventHandler(_messageBusMock.Object, _context, _loggerMock.Object);
    }

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

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

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
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

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

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<UserDeletedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ThrowsAsync(new InvalidOperationException("Message bus unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(domainEvent, CancellationToken.None)
        );

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
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

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

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

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(domainEvent, token);

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
    public async Task HandleAsync_ShouldLogHandlingMessage()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var user = new UserBuilder()
            .WithId(userId)
            .WithEmail("john@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserDeletedDomainEvent(userId, 1);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert - Verify initial handling log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling UserDeletedDomainEvent")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
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

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        var event1 = new UserDeletedDomainEvent(userId1, 1);
        var event2 = new UserDeletedDomainEvent(userId2, 1);
        var event3 = new UserDeletedDomainEvent(userId3, 1);

        // Act
        await _handler.HandleAsync(event1, CancellationToken.None);
        await _handler.HandleAsync(event2, CancellationToken.None);
        await _handler.HandleAsync(event3, CancellationToken.None);

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

    public void Dispose()
    {
        _context.Dispose();
    }
}
