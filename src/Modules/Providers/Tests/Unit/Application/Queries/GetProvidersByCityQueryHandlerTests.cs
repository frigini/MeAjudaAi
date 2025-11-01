using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

public class GetProvidersByCityQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProvidersByCityQueryHandler>> _loggerMock;
    private readonly GetProvidersByCityQueryHandler _handler;

    public GetProvidersByCityQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProvidersByCityQueryHandler>>();
        _handler = new GetProvidersByCityQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCity_ShouldReturnProviders()
    {
        // Arrange
        var city = "São Paulo";
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(Guid.NewGuid()),
            ProviderBuilder.Create().WithId(Guid.NewGuid())
        };

        var query = new GetProvidersByCityQuery(city);

        _providerRepositoryMock
            .Setup(r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllBeOfType<ProviderDto>();

        _providerRepositoryMock.Verify(
            r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var city = "Cidade Inexistente";
        var providers = new List<Provider>();

        var query = new GetProvidersByCityQuery(city);

        _providerRepositoryMock
            .Setup(r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithRepositoryException_ShouldReturnFailureResult()
    {
        // Arrange
        var city = "São Paulo";
        var query = new GetProvidersByCityQuery(city);

        _providerRepositoryMock
            .Setup(r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error getting providers");

        _providerRepositoryMock.Verify(
            r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithLargeResultSet_ShouldReturnAllProviders()
    {
        // Arrange
        var city = "São Paulo";
        var providers = Enumerable.Range(1, 100)
            .Select(_ => (Provider)ProviderBuilder.Create().WithId(Guid.NewGuid()))
            .ToList();

        var query = new GetProvidersByCityQuery(city);

        _providerRepositoryMock
            .Setup(r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(100);

        _providerRepositoryMock.Verify(
            r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("São Paulo")]
    [InlineData("Rio de Janeiro")]
    [InlineData("Brasília")]
    [InlineData("Belo Horizonte")]
    public async Task HandleAsync_WithDifferentCities_ShouldWork(string city)
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(Guid.NewGuid())
        };

        var query = new GetProvidersByCityQuery(city);

        _providerRepositoryMock
            .Setup(r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        _providerRepositoryMock.Verify(
            r => r.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var city = "São Paulo";
        var providers = new List<Provider>();
        var query = new GetProvidersByCityQuery(city);
        var cancellationToken = new CancellationToken();

        _providerRepositoryMock
            .Setup(r => r.GetByCityAsync(city, cancellationToken))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.GetByCityAsync(city, cancellationToken),
            Times.Once);
    }
}
