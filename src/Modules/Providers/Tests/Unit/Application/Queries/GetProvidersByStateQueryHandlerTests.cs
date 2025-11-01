using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Queries;

public class GetProvidersByStateQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProvidersByStateQueryHandler>> _loggerMock;
    private readonly GetProvidersByStateQueryHandler _handler;

    public GetProvidersByStateQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProvidersByStateQueryHandler>>();
        _handler = new GetProvidersByStateQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidState_ShouldReturnProviders()
    {
        // Arrange
        var state = "SP";
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(Guid.NewGuid()),
            ProviderBuilder.Create().WithId(Guid.NewGuid()),
            ProviderBuilder.Create().WithId(Guid.NewGuid())
        };

        var query = new GetProvidersByStateQuery(state);

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().AllBeOfType<ProviderDto>();

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var state = "AC"; // Estado com poucos prestadores
        var providers = new List<Provider>();

        var query = new GetProvidersByStateQuery(state);

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithRepositoryException_ShouldReturnFailureResult()
    {
        // Arrange
        var state = "SP";
        var query = new GetProvidersByStateQuery(state);

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error getting providers");

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithLargeResultSet_ShouldReturnAllProviders()
    {
        // Arrange
        var state = "SP";
        var providers = Enumerable.Range(1, 150)
            .Select(_ => (Provider)ProviderBuilder.Create().WithId(Guid.NewGuid()))
            .ToList();

        var query = new GetProvidersByStateQuery(state);

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(150);

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("SP")]
    [InlineData("RJ")]
    [InlineData("MG")]
    [InlineData("RS")]
    [InlineData("PR")]
    [InlineData("SC")]
    [InlineData("BA")]
    [InlineData("GO")]
    [InlineData("DF")]
    [InlineData("ES")]
    public async Task HandleAsync_WithDifferentStates_ShouldWork(string state)
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(Guid.NewGuid())
        };

        var query = new GetProvidersByStateQuery(state);

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithStateAbbreviation_ShouldWork()
    {
        // Arrange
        var state = "SP";
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(Guid.NewGuid())
        };

        var query = new GetProvidersByStateQuery(state);

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithFullStateName_ShouldWork()
    {
        // Arrange
        var state = "São Paulo";
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithId(Guid.NewGuid()),
            ProviderBuilder.Create().WithId(Guid.NewGuid())
        };

        var query = new GetProvidersByStateQuery(state);

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var state = "SP";
        var providers = new List<Provider>();
        var query = new GetProvidersByStateQuery(state);
        var cancellationToken = new CancellationToken();

        _providerRepositoryMock
            .Setup(r => r.GetByStateAsync(state, cancellationToken))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.GetByStateAsync(state, cancellationToken),
            Times.Once);
    }
}
