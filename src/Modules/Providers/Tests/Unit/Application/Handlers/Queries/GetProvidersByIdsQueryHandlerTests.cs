using FluentAssertions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;

using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProvidersByIdsQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProvidersByIdsQueryHandler>> _loggerMock;
    private readonly GetProvidersByIdsQueryHandler _handler;

    public GetProvidersByIdsQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProvidersByIdsQueryHandler>>();
        _handler = new GetProvidersByIdsQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidIds_ShouldReturnProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var provider2 = new ProviderBuilder().Build();
        var providerIds = new[] { provider1.Id.Value, provider2.Id.Value };

        _providerQueriesMock
            .Setup(x => x.GetByIdsAsync(It.Is<IReadOnlyList<Guid>>(ids => ids.SequenceEqual(providerIds)), It.IsAny<CancellationToken>()))
            .ReturnsAsync([provider1, provider2]);

        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(p => p.Id).Should().Contain(provider1.Id.Value);
        result.Value.Select(p => p.Id).Should().Contain(provider2.Id.Value);

        _providerQueriesMock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyIdsList_ShouldReturnEmptyList()
    {
        // Arrange
        var providerIds = Array.Empty<Guid>();

        _providerQueriesMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerQueriesMock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSomeIdsNotFound_ShouldReturnOnlyFoundProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().Build();
        var nonExistentId = Guid.NewGuid();
        var providerIds = new[] { provider1.Id.Value, nonExistentId };

        _providerQueriesMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([provider1]); // Only one found

        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(provider1.Id.Value);

        _providerQueriesMock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoIdsFound_ShouldReturnEmptyList()
    {
        // Arrange
        var providerIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _providerQueriesMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _providerQueriesMock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var providerIds = new[] { Guid.NewGuid() };
        var exception = new Exception("Database error");

        _providerQueriesMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.ErrorRetrievingProviders);

        _providerQueriesMock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var providerIds = new[] { Guid.NewGuid() };
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _providerQueriesMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), cancellationToken))
            .ReturnsAsync([]);

        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        await _handler.HandleAsync(query, cancellationToken);

        // Assert
        _providerQueriesMock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateIds_ShouldHandleCorrectly()
    {
        // Arrange
        var provider = new ProviderBuilder().Build();
        var providerIds = new[] { provider.Id.Value, provider.Id.Value }; // Duplicate ID

        _providerQueriesMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([provider]);

        var query = new GetProvidersByIdsQuery(providerIds);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);

        _providerQueriesMock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
