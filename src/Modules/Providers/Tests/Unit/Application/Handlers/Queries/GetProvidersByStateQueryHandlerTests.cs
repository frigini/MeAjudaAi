using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProvidersByStateQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProvidersByStateQueryHandler>> _loggerMock;
    private readonly GetProvidersByStateQueryHandler _handler;

    public GetProvidersByStateQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProvidersByStateQueryHandler>>();
        _handler = new GetProvidersByStateQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidState_ShouldReturnProviders()
    {
        // Arrange
        var state = "SP";
        var providers = new[]
        {
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build()
        };

        _providerQueriesMock
            .Setup(x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var query = new GetProvidersByStateQuery(state);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        _providerQueriesMock.Verify(
            x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullState_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetProvidersByStateQuery(null!);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.StateParameterRequired);

        _providerQueriesMock.Verify(
            x => x.GetByStateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyState_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetProvidersByStateQuery(string.Empty);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.StateParameterRequired);

        _providerQueriesMock.Verify(
            x => x.GetByStateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceState_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetProvidersByStateQuery("   ");

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.StateParameterRequired);

        _providerQueriesMock.Verify(
            x => x.GetByStateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoStateProviders_ShouldReturnEmptyList()
    {
        // Arrange
        var state = "XX";

        _providerQueriesMock
            .Setup(x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetProvidersByStateQuery(state);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerQueriesMock.Verify(
            x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var state = "SP";
        var exception = new Exception("Database error");

        _providerQueriesMock
            .Setup(x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var query = new GetProvidersByStateQuery(state);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var state = "SP";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _providerQueriesMock
            .Setup(x => x.GetByStateAsync(state, cancellationToken))
            .ReturnsAsync([]);

        var query = new GetProvidersByStateQuery(state);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _providerQueriesMock.Verify(
            x => x.GetByStateAsync(state, cancellationToken),
            Times.Once);
    }
}
