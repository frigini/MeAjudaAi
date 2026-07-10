using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class SuspendProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<SuspendProviderCommandHandler>> _loggerMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly SuspendProviderCommandHandler _handler;

    public SuspendProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<SuspendProviderCommandHandler>>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();

        _localizerMock
            .Setup(x => x[It.Is<string>(s => s == "ProviderNotFound")])
            .Returns(new LocalizedString("ProviderNotFound", "Prestador não encontrado."));
        _localizerMock
            .Setup(x => x[It.Is<string>(s => s == "SuspensionReasonRequired")])
            .Returns(new LocalizedString("SuspensionReasonRequired", "Motivo da suspensão é obrigatório."));
        _localizerMock
            .Setup(x => x[It.Is<string>(s => s == "SuspendedByRequired")])
            .Returns(new LocalizedString("SuspendedByRequired", "Responsável pela suspensão é obrigatório."));

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new SuspendProviderCommandHandler(
            _uowMock.Object,
            _loggerMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldSuspendProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        // Provider precisa estar Active para ser suspenso
        provider.CompleteBasicInfo();
        provider.Activate();

        var command = new SuspendProviderCommand(
            providerId,
            "admin@test.com",
            "Policy violation");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(
            $"Expected success but got error: {(result.IsFailure ? result.Error.Message : "unknown")}");
        provider.Status.Should().Be(EProviderStatus.Suspended);
        provider.SuspensionReason.Should().Be("Policy violation");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Policy violation");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Prestador não encontrado.");

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            reason!);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Motivo da suspensão é obrigatório.");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptySuspendedBy_ShouldReturnFailure(string? suspendedBy)
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            suspendedBy!,
            "Policy violation");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Responsável pela suspensão é obrigatório.");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldThrow()
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Policy violation");

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenDomainValidationFails_ShouldThrow()
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Policy violation");

        // Create provider in PendingBasicInfo status (default after creation)
        // Attempting to suspend will fail with domain exception because only Active providers can be suspended
        var provider = ProviderBuilder.Create()
            .WithType(EProviderType.Individual)
            .Build();

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ProviderDomainException>(() => _handler.HandleAsync(command, CancellationToken.None));
        exception.Message.Should().StartWith("Invalid status transition");

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}