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
public class UserProfileUpdatedDomainEventHandlerTests : BaseInMemoryDatabaseTest<UsersDbContext>
{
    private readonly Mock<IMessageBus> _messageBusMock;

    public UserProfileUpdatedDomainEventHandlerTests() : base(options => new UsersDbContext(options, null!))
    {
        _messageBusMock = new Mock<IMessageBus>();
    }

    private UserProfileUpdatedDomainEventHandler CreateHandler() =>
        new(_messageBusMock.Object, DbContext, NullLogger<UserProfileUpdatedDomainEventHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WithValidUser_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("updated@example.com")
            .WithFirstName("Updated")
            .WithLastName("Name")
            .Build();

        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserProfileUpdatedDomainEvent(
            user.Id.Value,
            1,
            "Updated",
            "Name"
        );

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

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
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ShouldLogWarningAndNotPublish()
    {
        // Arrange
        var userId = UuidGenerator.NewId();
        var domainEvent = new UserProfileUpdatedDomainEvent(userId, 1, "Non", "Existent");

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<UserProfileUpdatedIntegrationEvent>(),
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

        var domainEvent = new UserProfileUpdatedDomainEvent(user.Id.Value, 1, "Test", "User");

        _messageBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<UserProfileUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ))
            .ThrowsAsync(new InvalidOperationException("Message bus unavailable"));

        // Act & Assert
        var handler = CreateHandler();
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(domainEvent, CancellationToken.None)
        );

        Assert.StartsWith("Error handling UserProfileUpdatedDomainEvent for user", ex.Message);
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal("Message bus unavailable", ex.InnerException.Message);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToMessageBus()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var domainEvent = new UserProfileUpdatedDomainEvent(user.Id.Value, 1, "Test", "User");
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Act
        var handler = CreateHandler();
        await handler.HandleAsync(domainEvent, token);

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
}
