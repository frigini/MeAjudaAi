using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProviderByUserIdQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProviderByUserIdQueryHandler>> _loggerMock;
    private readonly GetProviderByUserIdQueryHandler _handler;

    public GetProviderByUserIdQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProviderByUserIdQueryHandler>>();
        _handler = new GetProviderByUserIdQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidUserId_ShouldReturnProviderDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithUserId(userId)
            .Build();

        _providerQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByUserIdQuery(userId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(userId);

        _providerQueriesMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUserId_ShouldReturnSuccessWithNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _providerQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Providers.Domain.Entities.Provider?)null);

        var query = new GetProviderByUserIdQuery(userId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _providerQueriesMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _providerQueriesMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetProviderByUserIdQuery(userId);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(query, CancellationToken.None));
    }
}
