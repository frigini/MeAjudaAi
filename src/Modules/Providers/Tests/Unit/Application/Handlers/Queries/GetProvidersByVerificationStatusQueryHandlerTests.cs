using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;

using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetProvidersByVerificationStatusQueryHandlerTests
{
    private readonly Mock<IProviderQueries> _providerQueriesMock;
    private readonly Mock<ILogger<GetProvidersByVerificationStatusQueryHandler>> _loggerMock;
    private readonly GetProvidersByVerificationStatusQueryHandler _handler;

    public GetProvidersByVerificationStatusQueryHandlerTests()
    {
        _providerQueriesMock = new Mock<IProviderQueries>();
        _loggerMock = new Mock<ILogger<GetProvidersByVerificationStatusQueryHandler>>();
        _handler = new GetProvidersByVerificationStatusQueryHandler(_providerQueriesMock.Object, _loggerMock.Object);
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

        _providerQueriesMock
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

        _providerQueriesMock.Verify(
            x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithStatusNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        var status = EVerificationStatus.Verified;

        _providerQueriesMock
            .Setup(x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>());

        var query = new GetProvidersByVerificationStatusQuery(status);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _providerQueriesMock.Verify(
            x => x.GetByVerificationStatusAsync(status, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var status = EVerificationStatus.Pending;

        _providerQueriesMock
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
