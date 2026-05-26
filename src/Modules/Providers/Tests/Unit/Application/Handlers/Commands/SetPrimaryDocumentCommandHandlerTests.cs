using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;

using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class SetPrimaryDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<MeAjudaAi.Modules.Providers.Domain.Entities.Provider, MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<SetPrimaryDocumentCommandHandler>> _mockLogger;
    private readonly SetPrimaryDocumentCommandHandler _handler;

    public SetPrimaryDocumentCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<MeAjudaAi.Modules.Providers.Domain.Entities.Provider, MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>>();
        _mockLogger = new Mock<ILogger<SetPrimaryDocumentCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<MeAjudaAi.Modules.Providers.Domain.Entities.Provider, MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new SetPrimaryDocumentCommandHandler(_uowMock.Object, _mockLogger.Object);
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

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        provider.Documents.Should().Contain(d => d.DocumentType == EDocumentType.CNPJ && d.IsPrimary);

        _providerRepositoryMock.Verify(r => r.TryFindAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ReturnsNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new SetPrimaryDocumentCommand(providerId, EDocumentType.CPF);

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Providers.Domain.Entities.Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("not found");

        _providerRepositoryMock.Verify(r => r.TryFindAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()), Times.Once);
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

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
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

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

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

        _providerRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Error setting primary document");
    }
}
