using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class GetProvidersByTypeQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProvidersByTypeQueryHandler>> _loggerMock;
    private readonly GetProvidersByTypeQueryHandler _handler;

    public GetProvidersByTypeQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProvidersByTypeQueryHandler>>();
        _handler = new GetProvidersByTypeQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData(EProviderType.Individual)]
    [InlineData(EProviderType.Company)]
    public async Task HandleAsync_WithValidType_ShouldReturnProviders(EProviderType providerType)
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithType(providerType),
            ProviderBuilder.Create().WithType(providerType),
            ProviderBuilder.Create().WithType(providerType)
        };

        var query = new GetProvidersByTypeQuery(providerType);

        _providerRepositoryMock
            .Setup(r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value.Should().AllSatisfy(p => p.Type.Should().Be(providerType));

        _providerRepositoryMock.Verify(
            r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoProvidersFound_ShouldReturnEmptyList()
    {
        // Arrange
        var providerType = EProviderType.Individual;
        var providers = new List<Provider>();
        var query = new GetProvidersByTypeQuery(providerType);

        _providerRepositoryMock
            .Setup(r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerType = EProviderType.Individual;
        var query = new GetProvidersByTypeQuery(providerType);

        _providerRepositoryMock
            .Setup(r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("An error occurred while retrieving providers");

        _providerRepositoryMock.Verify(
            r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithMixedTypes_ShouldReturnOnlyMatchingType()
    {
        // Arrange
        var targetType = EProviderType.Individual;
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithType(targetType),
            ProviderBuilder.Create().WithType(targetType)
        };

        var query = new GetProvidersByTypeQuery(targetType);

        _providerRepositoryMock
            .Setup(r => r.GetByTypeAsync(targetType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(p => p.Type.Should().Be(targetType));

        _providerRepositoryMock.Verify(
            r => r.GetByTypeAsync(targetType, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var providerType = EProviderType.Company;
        var providers = new List<Provider>();
        var query = new GetProvidersByTypeQuery(providerType);
        var cancellationToken = new CancellationToken();

        _providerRepositoryMock
            .Setup(r => r.GetByTypeAsync(providerType, cancellationToken))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.GetByTypeAsync(providerType, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithLargeResultSet_ShouldReturnAllProviders()
    {
        // Arrange
        var providerType = EProviderType.Individual;
        var providers = new List<Provider>();

        // Criar uma lista grande de prestadores
        for (int i = 0; i < 100; i++)
        {
            providers.Add(ProviderBuilder.Create().WithType(providerType));
        }

        var query = new GetProvidersByTypeQuery(providerType);

        _providerRepositoryMock
            .Setup(r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(100);
        result.Value.Should().AllSatisfy(p => p.Type.Should().Be(providerType));

        _providerRepositoryMock.Verify(
            r => r.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
