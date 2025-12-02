using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Queries;

[Trait("Category", "Unit")]
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
        var providers = new[]
        {
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build()
        };

        _providerRepositoryMock
            .Setup(x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var query = new GetProvidersByCityQuery(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        _providerRepositoryMock.Verify(
            x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoCityProviders_ShouldReturnEmptyList()
    {
        // Arrange
        var city = "Cidade Inexistente";

        _providerRepositoryMock
            .Setup(x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetProvidersByCityQuery(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var city = "São Paulo";
        var exception = new Exception("Database error");

        _providerRepositoryMock
            .Setup(x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var query = new GetProvidersByCityQuery(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("error occurred");

        _providerRepositoryMock.Verify(
            x => x.GetByCityAsync(city, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var city = "São Paulo";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _providerRepositoryMock
            .Setup(x => x.GetByCityAsync(city, cancellationToken))
            .ReturnsAsync([]);

        var query = new GetProvidersByCityQuery(city);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _providerRepositoryMock.Verify(
            x => x.GetByCityAsync(city, cancellationToken),
            Times.Once);
    }
}
