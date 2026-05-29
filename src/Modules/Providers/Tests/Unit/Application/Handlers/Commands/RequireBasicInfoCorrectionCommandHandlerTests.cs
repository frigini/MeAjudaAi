using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class RequireBasicInfoCorrectionCommandHandlerTests
{
    private readonly Mock<IProviderUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<RequireBasicInfoCorrectionCommandHandler>> _loggerMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly RequireBasicInfoCorrectionCommandHandler _handler;

    public RequireBasicInfoCorrectionCommandHandlerTests()
    {
        _uowMock = new Mock<IProviderUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<RequireBasicInfoCorrectionCommandHandler>>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        
        // Setup localizer to return the key name
        _localizerMock.Setup(l => l[It.IsAny<string>()]).Returns((string name) => new LocalizedString(name, name));

        _handler = new RequireBasicInfoCorrectionCommandHandler(_uowMock.Object, _loggerMock.Object, _localizerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .Build();

        provider.CompleteBasicInfo(); // Transition to PendingDocumentVerification

        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: "Missing required information in business profile",
            RequestedBy: "verifier@test.com"
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.PendingBasicInfo);

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: "Missing information",
            RequestedBy: "verifier@test.com"
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.ToString().Should().Contain("ProviderNotFound");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidReason_ShouldReturnFailureResult(string? invalidReason)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: invalidReason!,
            RequestedBy: "verifier@test.com"
        );

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.ToString().Should().Contain("CorrectionReasonRequired");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidRequestedBy_ShouldReturnFailureResult(string? invalidRequestedBy)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: "Missing information",
            RequestedBy: invalidRequestedBy!
        );

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.ToString().Should().Contain("RequestedByRequired");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(EProviderStatus.PendingBasicInfo)]
    [InlineData(EProviderStatus.Active)]
    [InlineData(EProviderStatus.Suspended)]
    [InlineData(EProviderStatus.Rejected)]
    public async Task HandleAsync_WhenProviderNotInPendingDocumentVerification_ShouldReturnDomainValidationMessage(EProviderStatus status)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .Build();

        // Transition provider to the specified status
        if (status == EProviderStatus.Active)
        {
            provider.CompleteBasicInfo();
            provider.Activate();
        }
        else if (status == EProviderStatus.Suspended)
        {
            provider.CompleteBasicInfo();
            provider.Activate();
            provider.Suspend("Test reason", "admin");
        }
        else if (status == EProviderStatus.Rejected)
        {
            provider.Reject("Test reason", "admin");
        }
        // PendingBasicInfo is default, no action needed

        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: "Missing information",
            RequestedBy: "verifier@test.com"
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.ToString().Should().Contain("Can only require basic info correction during document verification");

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: "Missing information",
            RequestedBy: "verifier@test.com"
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.ToString().Should().Contain("BasicInfoCorrectionError");

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldTransitionProviderToPendingBasicInfo()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .Build();

        provider.CompleteBasicInfo(); // Transition to PendingDocumentVerification

        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: "Address information incomplete",
            RequestedBy: "admin@test.com"
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.PendingBasicInfo);
        provider.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_ShouldEmitDomainEvent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .Build();

        provider.CompleteBasicInfo(); // Transition to PendingDocumentVerification

        var command = new RequireBasicInfoCorrectionCommand(
            ProviderId: providerId,
            Reason: "Contact information needs verification",
            RequestedBy: "verifier@test.com"
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.DomainEvents.Should().NotBeEmpty();

        var correctionEvent = provider.DomainEvents
            .OfType<ProviderBasicInfoCorrectionRequiredDomainEvent>()
            .Should().ContainSingle().Subject;

        correctionEvent.Reason.Should().Be("Contact information needs verification");
        correctionEvent.RequestedBy.Should().Be("verifier@test.com");
    }
}
