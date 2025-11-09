using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class GetProviderByUserIdQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProviderByUserIdQueryHandler>> _loggerMock;
    private readonly GetProviderByUserIdQueryHandler _handler;

    public GetProviderByUserIdQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProviderByUserIdQueryHandler>>();
        _handler = new GetProviderByUserIdQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ShouldReturnProviderSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var query = new GetProviderByUserIdQuery(userId);

        _providerRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetProviderByUserIdQuery(userId);

        _providerRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetProviderByUserIdQuery(userId);

        _providerRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Error getting provider");
    }
}
