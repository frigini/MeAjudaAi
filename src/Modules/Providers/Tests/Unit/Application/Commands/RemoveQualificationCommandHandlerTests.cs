using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class RemoveQualificationCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<RemoveQualificationCommandHandler>> _loggerMock;
    private readonly RemoveQualificationCommandHandler _handler;

    public RemoveQualificationCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<RemoveQualificationCommandHandler>>();
        _handler = new RemoveQualificationCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var qualificationName = "Certificação AWS";
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithQualification(
                qualificationName,
                "AWS Solutions Architect Associate",
                "Amazon Web Services",
                new DateTime(2023, 1, 1),
                new DateTime(2025, 1, 1),
                "AWS-SAA-123456"
            );

        var command = new RemoveQualificationCommand(
            ProviderId: providerId,
            QualificationName: qualificationName
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new RemoveQualificationCommand(
            ProviderId: providerId,
            QualificationName: "Certificação AWS"
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Provider not found");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenQualificationNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId); // Sem qualificações

        var command = new RemoveQualificationCommand(
            ProviderId: providerId,
            QualificationName: "Certificação Inexistente"
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("An error occurred while removing the qualification");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new RemoveQualificationCommand(
            ProviderId: providerId,
            QualificationName: "Certificação AWS"
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("An error occurred while removing the qualification");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_WithInvalidQualificationName_ShouldReturnFailureResult(string qualificationName)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);

        var command = new RemoveQualificationCommand(
            ProviderId: providerId,
            QualificationName: qualificationName
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("An error occurred while removing the qualification");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleQualifications_ShouldRemoveOnlySpecified()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var qualificationToRemove = "Certificação AWS";
        var qualificationToKeep = "Certificação Azure";

        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithQualification(
                qualificationToRemove,
                "AWS Solutions Architect Associate",
                "Amazon Web Services",
                new DateTime(2023, 1, 1),
                new DateTime(2025, 1, 1),
                "AWS-SAA-123456"
            )
            .WithQualification(
                qualificationToKeep,
                "Azure Solutions Architect Expert",
                "Microsoft",
                new DateTime(2023, 6, 1),
                new DateTime(2025, 6, 1),
                "AZ-305-789012"
            );

        var command = new RemoveQualificationCommand(
            ProviderId: providerId,
            QualificationName: qualificationToRemove
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
