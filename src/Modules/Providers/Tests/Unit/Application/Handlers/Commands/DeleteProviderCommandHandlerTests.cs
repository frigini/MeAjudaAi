using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class DeleteProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly FakeTimeProvider _timeProvider;
    private readonly Mock<ILogger<DeleteProviderCommandHandler>> _loggerMock;
    private readonly DeleteProviderCommandHandler _handler;

    public DeleteProviderCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        _loggerMock = new Mock<ILogger<DeleteProviderCommandHandler>>();
        _handler = new DeleteProviderCommandHandler(_providerRepositoryMock.Object, _timeProvider, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidProvider_ShouldDeleteProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var command = new DeleteProviderCommand(providerId, "admin@test.com");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            x => x.UpdateAsync(provider, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Providers.Domain.Entities.Provider?)null);

        var command = new DeleteProviderCommand(providerId, "admin@test.com");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("not found");

        _providerRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.Entities.Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
