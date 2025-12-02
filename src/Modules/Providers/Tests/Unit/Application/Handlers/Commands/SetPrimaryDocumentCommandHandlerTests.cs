using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class SetPrimaryDocumentCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _mockRepository;
    private readonly Mock<ILogger<SetPrimaryDocumentCommandHandler>> _mockLogger;
    private readonly SetPrimaryDocumentCommandHandler _handler;

    public SetPrimaryDocumentCommandHandlerTests()
    {
        _mockRepository = new Mock<IProviderRepository>();
        _mockLogger = new Mock<ILogger<SetPrimaryDocumentCommandHandler>>();
        _handler = new SetPrimaryDocumentCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidDocument_SetsPrimaryDocument()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithDocument("12345678901", EDocumentType.CPF)
            .WithDocument("12345678000195", EDocumentType.CNPJ)
            .Build();

        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CNPJ);

        _mockRepository.Setup(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockRepository.Setup(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        provider.Documents.Should().Contain(d => d.DocumentType == EDocumentType.CNPJ && d.IsPrimary);

        _mockRepository.Verify(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ReturnsNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _mockRepository.Setup(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Providers.Domain.Entities.Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("not found");

        _mockRepository.Verify(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ReturnsNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithDocument("12345678901", EDocumentType.CPF)
            .Build();

        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CNPJ);

        _mockRepository.Setup(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task HandleAsync_WithDomainException_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithDocument("12345678901", EDocumentType.CPF)
            .Build();

        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _mockRepository.Setup(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockRepository.Setup(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProviderDomainException("Business rule violation"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Business rule violation");
    }

    [Fact]
    public async Task HandleAsync_WithRepositoryException_ReturnsInternalError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithDocument("12345678901", EDocumentType.CPF)
            .Build();

        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _mockRepository.Setup(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockRepository.Setup(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Error setting primary document");
    }
}
