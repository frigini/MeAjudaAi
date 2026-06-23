using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProvidersByTypeQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProvidersByTypeQueryHandler>> _loggerMock;
    private readonly GetProvidersByTypeQueryHandler _handler;

    public GetProvidersByTypeQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProvidersByTypeQueryHandler>>();
        _handler = new GetProvidersByTypeQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidType_ShouldReturnProviderList()
    {
        // Arrange
        var providerType = EProviderType.Individual;
        var providers = new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>
        {
            new ProviderBuilder().WithType(providerType).Build(),
            new ProviderBuilder().WithType(providerType).Build()
        };

        _providerQueriesMock
            .Setup(x => x.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var query = new GetProvidersByTypeQuery(providerType);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(p => p.Type == providerType);

        _providerQueriesMock.Verify(
            x => x.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithTypeNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        var providerType = EProviderType.Company;

        _providerQueriesMock
            .Setup(x => x.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>());

        var query = new GetProvidersByTypeQuery(providerType);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _providerQueriesMock.Verify(
            x => x.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var providerType = EProviderType.Individual;

        _providerQueriesMock
            .Setup(x => x.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetProvidersByTypeQuery(providerType);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.ErrorRetrievingProviders);

        _providerQueriesMock.Verify(
            x => x.GetByTypeAsync(providerType, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
