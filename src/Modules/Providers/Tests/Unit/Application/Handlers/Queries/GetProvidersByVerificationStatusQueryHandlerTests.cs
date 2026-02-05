using FluentAssertions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
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

    [Fact]
    public async Task HandleAsync_WithValidStatus_ShouldReturnProviderList()
    {
        // Arrange
        var status = EVerificationStatus.Pending;
        var providers = new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>
        {
            new ProviderBuilder().WithVerificationStatus(status).Build(),
            new ProviderBuilder().WithVerificationStatus(status).Build()
        };

        _providerRepositoryMock
            .Setup(x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var query = new GetProvidersByVerificationStatusQuery(status);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(p => p.VerificationStatus == status);

        _providerRepositoryMock.Verify(
            x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithStatusNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        var status = EVerificationStatus.Verified;

        _providerRepositoryMock
            .Setup(x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>());

        var query = new GetProvidersByVerificationStatusQuery(status);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _providerRepositoryMock.Verify(
            x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var status = EVerificationStatus.Pending;

        _providerRepositoryMock
            .Setup(x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetProvidersByVerificationStatusQuery(status);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.ErrorRetrievingProviders);
    }
}
