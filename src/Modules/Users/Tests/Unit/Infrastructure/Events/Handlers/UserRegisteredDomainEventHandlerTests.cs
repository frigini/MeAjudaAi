using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Events.Handlers;

/// <summary>
/// Testes para o handler de eventos de domínio de registro de usuário
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class UserRegisteredDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<ILogger<UserRegisteredDomainEventHandler>> _loggerMock;
    private readonly UsersDbContext _context;
    private readonly UserRegisteredDomainEventHandler _handler;

    public UserRegisteredDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        _loggerMock = new Mock<ILogger<UserRegisteredDomainEventHandler>>();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsersDbContext(options);
        _handler = new UserRegisteredDomainEventHandler(_messageBusMock.Object, _context, _loggerMock.Object);
    }

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

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserRegisteredDomainEvent(
            user.Id.Value,
            1,
            user.Email.Value,
            user.Username,
            "Test",
            "User"
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

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

        // Verify info log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully published UserRegistered")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
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
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserRegisteredIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );

        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusThrows_ShouldPropagateException()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

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

    [Fact]
    public async Task HandleAsync_WithUserWithKeycloakId_ShouldIncludeKeycloakId()
    {
        // Arrange
        var user = new UserBuilder()
            .WithKeycloakId("keycloak-123")
            .Build();

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserRegisteredDomainEvent(
            user.Id.Value,
            1,
            user.Email.Value,
            user.Username,
            "Test",
            "User"
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<UserRegisteredIntegrationEvent>(e =>
                    e.KeycloakId == "keycloak-123"
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
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

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
        await _handler.HandleAsync(domainEvent, cancellationToken);

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

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
