using FluentAssertions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Queries;

[Trait("Category", "Unit")]
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
        var providers = new[]
        {
            new ProviderBuilder().Build(),
            new ProviderBuilder().Build()
        };

        _providerRepositoryMock
            .Setup(x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var query = new GetProvidersByStateQuery(state);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        _providerRepositoryMock.Verify(
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
        result.Error!.Message.Should().Contain("obrigatório");

        _providerRepositoryMock.Verify(
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
        result.Error!.Message.Should().Contain("obrigatório");

        _providerRepositoryMock.Verify(
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
        result.Error!.Message.Should().Contain("obrigatório");

        _providerRepositoryMock.Verify(
            x => x.GetByStateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoStateProviders_ShouldReturnEmptyList()
    {
        // Arrange
        var state = "XX";

        _providerRepositoryMock
            .Setup(x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetProvidersByStateQuery(state);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var state = "SP";
        var exception = new Exception("Database error");

        _providerRepositoryMock
            .Setup(x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var query = new GetProvidersByStateQuery(state);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.ErrorRetrievingProviders);

        _providerRepositoryMock.Verify(
            x => x.GetByStateAsync(state, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var state = "SP";
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _providerRepositoryMock
            .Setup(x => x.GetByStateAsync(state, cancellationToken))
            .ReturnsAsync([]);

        var query = new GetProvidersByStateQuery(state);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _providerRepositoryMock.Verify(
            x => x.GetByStateAsync(state, cancellationToken),
            Times.Once);
    }
}
