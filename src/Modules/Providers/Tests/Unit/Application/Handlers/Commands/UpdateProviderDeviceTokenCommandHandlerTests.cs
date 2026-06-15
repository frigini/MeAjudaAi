using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class UpdateProviderDeviceTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<UpdateProviderDeviceTokenCommandHandler>> _loggerMock;
    private readonly UpdateProviderDeviceTokenCommandHandler _handler;

    public UpdateProviderDeviceTokenCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<UpdateProviderDeviceTokenCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new UpdateProviderDeviceTokenCommandHandler(
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        var command = new UpdateProviderDeviceTokenCommand(Guid.NewGuid(), "new-token");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsUnchanged_ShouldReturnSuccessWithoutSaving()
    {
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        provider.UpdateDeviceToken("same-token");

        var command = new UpdateProviderDeviceTokenCommand(providerId, "same-token");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WhenBothTokensNormalizeToNull_ShouldReturnSuccessWithoutSaving(string? newToken)
    {
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        // provider.DeviceToken is null by default

        var command = new UpdateProviderDeviceTokenCommand(providerId, newToken!);

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenChanges_ShouldUpdateAndSave()
    {
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        provider.UpdateDeviceToken("old-token");

        var command = new UpdateProviderDeviceTokenCommand(providerId, "new-token");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        provider.DeviceToken.Should().Be("new-token");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderHasTokenAndNewTokenIsWhitespace_ShouldUpdateAndSave()
    {
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder().WithId(providerId).Build();
        provider.UpdateDeviceToken("existing-token");

        var command = new UpdateProviderDeviceTokenCommand(providerId, "   ");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionOccurs_ShouldReturnFailure()
    {
        var command = new UpdateProviderDeviceTokenCommand(Guid.NewGuid(), "new-token");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WhenOperationCanceled_ShouldRethrow()
    {
        var command = new UpdateProviderDeviceTokenCommand(Guid.NewGuid(), "new-token");
        var cts = new CancellationTokenSource();

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => await _handler.HandleAsync(command, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
