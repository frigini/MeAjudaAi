using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class AddDocumentCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<AddDocumentCommandHandler>> _loggerMock;
    private readonly AddDocumentCommandHandler _handler;

    public AddDocumentCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<AddDocumentCommandHandler>>();
        _handler = new AddDocumentCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);
        var command = new AddDocumentCommand(
            ProviderId: providerId,
            DocumentNumber: "11144477735",
            DocumentType: EDocumentType.CPF,
            FileName: "doc.pdf",
            FileUrl: "https://storage/doc.pdf"
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
        result.Value.FileName.Should().Be("doc.pdf");
        result.Value.FileUrl.Should().Be("https://storage/doc.pdf");

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
        var command = new AddDocumentCommand(
            ProviderId: providerId,
            DocumentNumber: "11144477735",
            DocumentType: EDocumentType.CPF
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Fornecedor não encontrado");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("", EDocumentType.CPF)]
    [InlineData("12345", EDocumentType.CPF)]
    [InlineData("invalid", EDocumentType.CNPJ)]
    public async Task HandleAsync_WithInvalidDocumentNumber_ShouldReturnFailureResult(string documentNumber, EDocumentType documentType)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);
        var command = new AddDocumentCommand(
            ProviderId: providerId,
            DocumentNumber: documentNumber,
            DocumentType: documentType
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao adicionar o documento");

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
        var command = new AddDocumentCommand(
            ProviderId: providerId,
            DocumentNumber: "11144477735",
            DocumentType: EDocumentType.CPF
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao adicionar o documento");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateDocumentType_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithDocument("98765432100", EDocumentType.CPF);

        var command = new AddDocumentCommand(
            ProviderId: providerId,
            DocumentNumber: "11144477735",
            DocumentType: EDocumentType.CPF
        );

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao adicionar o documento");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
