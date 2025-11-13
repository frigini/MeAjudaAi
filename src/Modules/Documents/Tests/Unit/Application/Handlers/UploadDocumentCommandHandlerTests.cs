using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers;

public class UploadDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IBlobStorageService> _blobStorageMock;
    private readonly Mock<ILogger<UploadDocumentCommandHandler>> _loggerMock;
    private readonly UploadDocumentCommandHandler _handler;

    public UploadDocumentCommandHandlerTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _blobStorageMock = new Mock<IBlobStorageService>();
        _loggerMock = new Mock<ILogger<UploadDocumentCommandHandler>>();
        _handler = new UploadDocumentCommandHandler(
            _repositoryMock.Object,
            _blobStorageMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateDocument_AndReturnUploadUrl()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var fileName = "id-card.pdf";
        var documentType = DocumentType.IdentityDocument;
        var expectedUploadUrl = "https://storage.example.com/upload?token=abc123";

        var command = new UploadDocumentCommand
        {
            ProviderId = providerId,
            FileName = fileName,
            DocumentType = documentType,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _blobStorageMock
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUploadUrl);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().NotBeEmpty();
        result.UploadUrl.Should().Be(expectedUploadUrl);
        result.FileUrl.Should().NotBeNullOrEmpty();

        _repositoryMock.Verify(
            x => x.AddAsync(It.Is<Document>(d =>
                d.ProviderId == providerId &&
                d.FileName == fileName &&
                d.DocumentType == documentType &&
                d.Status == DocumentStatus.Uploaded), 
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldGenerateUniqueFileUrl()
    {
        // Arrange
        var command = new UploadDocumentCommand
        {
            ProviderId = Guid.NewGuid(),
            FileName = "test.pdf",
            DocumentType = DocumentType.ProofOfResidence,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _blobStorageMock
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://upload.url");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.FileUrl.Should().Contain(command.ProviderId.ToString());
        result.FileUrl.Should().Contain(".pdf");
    }

    [Fact]
    public async Task HandleAsync_ShouldLogInformation()
    {
        // Arrange
        var command = new UploadDocumentCommand
        {
            ProviderId = Guid.NewGuid(),
            FileName = "test.pdf",
            DocumentType = DocumentType.IdentityDocument,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _blobStorageMock
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://upload.url");

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
