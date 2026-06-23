using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProvidersQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProvidersQueryHandler>> _loggerMock;
    private readonly GetProvidersQueryHandler _handler;

    public GetProvidersQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProvidersQueryHandler>>();
        _handler = new GetProvidersQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var providers = new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>
        {
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build()
        };

        var pagedProviders = new PagedResult<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>
        {
            Items = providers,
            PageNumber = 1,
            PageSize = 10,
            TotalItems = 2
        };

        _providerQueriesMock
            .Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<MeAjudaAi.Modules.Providers.Domain.Enums.EProviderType?>(), It.IsAny<MeAjudaAi.Modules.Providers.Domain.Enums.EVerificationStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedProviders);

        var query = new GetProvidersQuery(1, 10);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        _providerQueriesMock
            .Setup(x => x.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<MeAjudaAi.Modules.Providers.Domain.Enums.EProviderType?>(), It.IsAny<MeAjudaAi.Modules.Providers.Domain.Enums.EVerificationStatus?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetProvidersQuery(1, 10);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Erro interno ao buscar prestadores");
    }
}
