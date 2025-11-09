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
public class GetProvidersByVerificationStatusQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<GetProvidersByVerificationStatusQueryHandler>> _loggerMock;
    private readonly GetProvidersByVerificationStatusQueryHandler _handler;

    public GetProvidersByVerificationStatusQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<GetProvidersByVerificationStatusQueryHandler>>();
        _handler = new GetProvidersByVerificationStatusQueryHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData(EVerificationStatus.Pending)]
    [InlineData(EVerificationStatus.InProgress)]
    [InlineData(EVerificationStatus.Verified)]
    [InlineData(EVerificationStatus.Rejected)]
    public async Task HandleAsync_WithValidStatus_ShouldReturnProviders(EVerificationStatus status)
    {
        // Arrange
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithVerificationStatus(status),
            ProviderBuilder.Create().WithVerificationStatus(status),
            ProviderBuilder.Create().WithVerificationStatus(status)
        };

        var query = new GetProvidersByVerificationStatusQuery(status);

        _providerRepositoryMock
            .Setup(r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value.Should().AllSatisfy(p => p.VerificationStatus.Should().Be(status));

        _providerRepositoryMock.Verify(
            r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoProvidersFound_ShouldReturnEmptyList()
    {
        // Arrange
        var status = EVerificationStatus.Verified;
        var providers = new List<Provider>();
        var query = new GetProvidersByVerificationStatusQuery(status);

        _providerRepositoryMock
            .Setup(r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var status = EVerificationStatus.Pending;
        var query = new GetProvidersByVerificationStatusQuery(status);

        _providerRepositoryMock
            .Setup(r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("An error occurred while retrieving providers");

        _providerRepositoryMock.Verify(
            r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithVerifiedStatus_ShouldReturnOnlyVerifiedProviders()
    {
        // Arrange
        var status = EVerificationStatus.Verified;
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithVerificationStatus(EVerificationStatus.Verified),
            ProviderBuilder.Create().WithVerificationStatus(EVerificationStatus.Verified)
        };

        var query = new GetProvidersByVerificationStatusQuery(status);

        _providerRepositoryMock
            .Setup(r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(p => p.VerificationStatus.Should().Be(EVerificationStatus.Verified));

        _providerRepositoryMock.Verify(
            r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithPendingStatus_ShouldReturnOnlyPendingProviders()
    {
        // Arrange
        var status = EVerificationStatus.Pending;
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithVerificationStatus(EVerificationStatus.Pending),
            ProviderBuilder.Create().WithVerificationStatus(EVerificationStatus.Pending),
            ProviderBuilder.Create().WithVerificationStatus(EVerificationStatus.Pending)
        };

        var query = new GetProvidersByVerificationStatusQuery(status);

        _providerRepositoryMock
            .Setup(r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value.Should().AllSatisfy(p => p.VerificationStatus.Should().Be(EVerificationStatus.Pending));

        _providerRepositoryMock.Verify(
            r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var status = EVerificationStatus.InProgress;
        var providers = new List<Provider>();
        var query = new GetProvidersByVerificationStatusQuery(status);
        var cancellationToken = new CancellationToken();

        _providerRepositoryMock
            .Setup(r => r.GetByVerificationStatusAsync(status, cancellationToken))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.GetByVerificationStatusAsync(status, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithRejectedStatus_ShouldReturnOnlyRejectedProviders()
    {
        // Arrange
        var status = EVerificationStatus.Rejected;
        var providers = new List<Provider>
        {
            ProviderBuilder.Create().WithVerificationStatus(EVerificationStatus.Rejected)
        };

        var query = new GetProvidersByVerificationStatusQuery(status);

        _providerRepositoryMock
            .Setup(r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        result.Value.Should().AllSatisfy(p => p.VerificationStatus.Should().Be(EVerificationStatus.Rejected));

        _providerRepositoryMock.Verify(
            r => r.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
