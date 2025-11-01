using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

public class GetProvidersByIdsQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProvidersByIdsQueryHandler>> _loggerMock;
    private readonly GetProvidersByIdsQueryHandler _handler;

    public GetProvidersByIdsQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProvidersByIdsQueryHandler>>();
        _handler = new GetProvidersByIdsQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidIds_ShouldReturnProviders()
    {
        // Arrange
        var providerIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(providerIds[0]),
            ProviderBuilder.Create().WithId(providerIds[1])
        };

        var query = new GetProvidersByIdsQuery(providerIds);

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllBeOfType<ProviderDto>();

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyIdsList_ShouldReturnEmptyList()
    {
        // Arrange
        var providerIds = new List<Guid>();
        var providers = new List<Provider>();

        var query = new GetProvidersByIdsQuery(providerIds);

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentIds_ShouldReturnEmptyList()
    {
        // Arrange
        var providerIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var providers = new List<Provider>(); // Nenhum provider encontrado

        var query = new GetProvidersByIdsQuery(providerIds);

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithPartialMatch_ShouldReturnFoundProviders()
    {
        // Arrange
        var providerIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        // Apenas 2 dos 3 providers existem
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(providerIds[0]),
            ProviderBuilder.Create().WithId(providerIds[2])
        };

        var query = new GetProvidersByIdsQuery(providerIds);

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithRepositoryException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerIds = new List<Guid>
        {
            Guid.NewGuid()
        };

        var query = new GetProvidersByIdsQuery(providerIds);

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error getting providers by IDs");

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithLargeIdList_ShouldReturnAllFoundProviders()
    {
        // Arrange
        var providerIds = Enumerable.Range(1, 50)
            .Select(_ => Guid.NewGuid())
            .ToList();

        var providers = providerIds.Take(40) // 40 dos 50 existem
            .Select(id => (Provider)ProviderBuilder.Create().WithId(id))
            .ToList();

        var query = new GetProvidersByIdsQuery(providerIds);

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(40);

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithSingleId_ShouldReturnSingleProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerIds = new List<Guid> { providerId };

        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(providerId)
        };

        var query = new GetProvidersByIdsQuery(providerIds);

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var providerIds = new List<Guid>
        {
            Guid.NewGuid()
        };

        var providers = new List<Provider>();
        var query = new GetProvidersByIdsQuery(providerIds);
        var cancellationToken = new CancellationToken();

        _providerRepositoryMock
            .Setup(r => r.GetByIdsAsync(providerIds, cancellationToken))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.GetByIdsAsync(providerIds, cancellationToken),
            Times.Once);
    }
}
