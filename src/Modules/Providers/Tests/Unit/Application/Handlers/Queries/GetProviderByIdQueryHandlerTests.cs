using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProviderByIdQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProviderByIdQueryHandler>> _loggerMock;
    private readonly GetProviderByIdQueryHandler _handler;

    public GetProviderByIdQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProviderByIdQueryHandler>>();
        _handler = new GetProviderByIdQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidId_ShouldReturnProviderDto()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        _providerQueriesMock
            .Setup(x => x.GetByIdAsync(It.Is<ProviderId>(id => id.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetProviderByIdQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id.Value);

        _providerQueriesMock.Verify(
            x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _providerQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetProviderByIdQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("não encontrado");

        _providerQueriesMock.Verify(
            x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
