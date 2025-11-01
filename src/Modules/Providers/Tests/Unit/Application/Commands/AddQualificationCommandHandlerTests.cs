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
public class AddQualificationCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<AddQualificationCommandHandler>> _loggerMock;
    private readonly AddQualificationCommandHandler _handler;

    public AddQualificationCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<AddQualificationCommandHandler>>();
        _handler = new AddQualificationCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: "AWS-SAA-123456"
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
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: "AWS-SAA-123456"
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

    [Theory]
    [InlineData("", "Valid Description", "Valid Organization", "DOC123")]
    [InlineData(null, "Valid Description", "Valid Organization", "DOC123")]
    [InlineData("   ", "Valid Description", "Valid Organization", "DOC123")]
    public async Task HandleAsync_WithInvalidQualificationData_ShouldReturnFailureResult(
        string? name, string description, string organization, string documentNumber)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: name,
            Description: description,
            IssuingOrganization: organization,
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: documentNumber
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error adding qualification");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidDates_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2025, 1, 1),
            ExpirationDate: new DateTime(2023, 1, 1), // Data de expiração anterior à data de emissão
            DocumentNumber: "AWS-SAA-123456"
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error adding qualification");

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
        var command = new AddQualificationCommand(
            ProviderId: providerId,
            Name: "Certificação AWS",
            Description: "AWS Solutions Architect Associate",
            IssuingOrganization: "Amazon Web Services",
            IssueDate: new DateTime(2023, 1, 1),
            ExpirationDate: new DateTime(2025, 1, 1),
            DocumentNumber: "AWS-SAA-123456"
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error adding qualification");
        result.Error.Message.Should().Contain("Database error");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
