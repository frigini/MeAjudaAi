using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class UpdateProviderDeviceTokenCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<UpdateProviderDeviceTokenCommandHandler>> _loggerMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly UpdateProviderDeviceTokenCommandHandler _handler;

    public UpdateProviderDeviceTokenCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<UpdateProviderDeviceTokenCommandHandler>>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();

        _localizerMock
            .Setup(x => x[It.Is<string>(s => s == "ProviderNotFound")])
            .Returns(new LocalizedString("ProviderNotFound", "Prestador não encontrado."));

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new UpdateProviderDeviceTokenCommandHandler(_uowMock.Object, _loggerMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateProviderDeviceTokenCommand(ProviderId: providerId, DeviceToken: "token-abc");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(404);
        result.Error.Message.Should().Be("Prestador não encontrado.");

        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenUnchanged_ShouldReturnSuccessWithoutSaving()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        const string existingToken = "existing-device-token";
        var provider = ProviderBuilder.Create().WithId(providerId).Build();
        provider.UpdateDeviceToken(existingToken);

        var command = new UpdateProviderDeviceTokenCommand(ProviderId: providerId, DeviceToken: existingToken);

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenChanged_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        const string newToken = "new-device-token";
        var provider = ProviderBuilder.Create().WithId(providerId).Build();

        var command = new UpdateProviderDeviceTokenCommand(ProviderId: providerId, DeviceToken: newToken);

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WhenWhitespaceToken_AndCurrentIsNull_ShouldReturnSuccessWithoutSaving(string whitespaceToken)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId).Build();
        // provider.DeviceToken is null by default

        var command = new UpdateProviderDeviceTokenCommand(ProviderId: providerId, DeviceToken: whitespaceToken);

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOperationCancelled_ShouldRethrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateProviderDeviceTokenCommand(ProviderId: providerId, DeviceToken: "token");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _handler.HandleAsync(command, cts.Token));
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateProviderDeviceTokenCommand(ProviderId: providerId, DeviceToken: "token");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }
}
