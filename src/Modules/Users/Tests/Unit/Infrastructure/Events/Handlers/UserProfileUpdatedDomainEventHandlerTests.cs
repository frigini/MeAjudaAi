using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class UserProfileUpdatedDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<ILogger<UserProfileUpdatedDomainEventHandler>> _loggerMock;
    private readonly UsersDbContext _context;
    private readonly UserProfileUpdatedDomainEventHandler _handler;

    public UserProfileUpdatedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        _loggerMock = new Mock<ILogger<UserProfileUpdatedDomainEventHandler>>();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsersDbContext(options);
        _handler = new UserProfileUpdatedDomainEventHandler(_messageBusMock.Object, _context, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidUser_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("updated@example.com")
            .WithFirstName("Updated")
            .WithLastName("Name")
            .Build();

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserProfileUpdatedDomainEvent(
            user.Id.Value,
            1,
            "Updated",
            "Name"
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<UserProfileUpdatedIntegrationEvent>(e =>
                    e.UserId == user.Id.Value &&
                    e.Email == user.Email.Value &&
                    e.FirstName == "Updated" &&
                    e.LastName == "Name"
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully published UserProfileUpdated")),
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
        var userId = Guid.NewGuid();
        var domainEvent = new UserProfileUpdatedDomainEvent(userId, 1, "Non", "Existent");

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserProfileUpdatedIntegrationEvent>(),
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

        var domainEvent = new UserProfileUpdatedDomainEvent(user.Id.Value, 1, "Test", "User");

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<UserProfileUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ThrowsAsync(new InvalidOperationException("Message bus unavailable"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(domainEvent, CancellationToken.None)
        );

        Assert.Equal("Message bus unavailable", ex.Message);

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
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToMessageBus()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var domainEvent = new UserProfileUpdatedDomainEvent(user.Id.Value, 1, "Test", "User");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        await _handler.HandleAsync(domainEvent, token);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserProfileUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.Is<CancellationToken>(ct => ct == token)
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
