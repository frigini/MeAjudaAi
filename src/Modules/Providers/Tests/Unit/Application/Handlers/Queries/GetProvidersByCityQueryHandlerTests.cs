using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProvidersByCityQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProvidersByCityQueryHandler>> _loggerMock;
    private readonly GetProvidersByCityQueryHandler _handler;

    public GetProvidersByCityQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProvidersByCityQueryHandler>>();
        _handler = new GetProvidersByCityQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCity_ShouldReturnProviders()
    {
        // Arrange
        var city = "São Paulo";
        var providers = new[]
        {
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build()
        };

        _providerQueriesMock
            .Setup(x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var query = new GetProvidersByCityQuery(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        _providerQueriesMock.Verify(
            x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoCityProviders_ShouldReturnEmptyList()
    {
        // Arrange
        var city = "Cidade Inexistente";

        _providerQueriesMock
            .Setup(x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetProvidersByCityQuery(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerQueriesMock.Verify(
            x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var city = "São Paulo";
        var exception = new Exception("Database error");

        _providerQueriesMock
            .Setup(x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var query = new GetProvidersByCityQuery(city);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var city = "São Paulo";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _providerQueriesMock
            .Setup(x => x.GetByCityAsync(city, cancellationToken))
            .ReturnsAsync([]);

        var query = new GetProvidersByCityQuery(city);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _providerQueriesMock.Verify(
            x => x.GetByCityAsync(city, cancellationToken),
            Times.Once);
    }
}
