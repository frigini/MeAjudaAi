using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Jobs;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class UploadDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<IBlobStorageService> _mockBlobStorage;
    private readonly Mock<IBackgroundJobService> _mockJobService;
    private readonly Mock<ILogger<UploadDocumentCommandHandler>> _mockLogger;
    private readonly UploadDocumentCommandHandler _handler;

    public UploadDocumentCommandHandlerTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockBlobStorage = new Mock<IBlobStorageService>();
        _mockJobService = new Mock<IBackgroundJobService>();
        _mockLogger = new Mock<ILogger<UploadDocumentCommandHandler>>();
        _handler = new UploadDocumentCommandHandler(
            _mockRepository.Object, 
            _mockBlobStorage.Object,
            _mockJobService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateDocument()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        var uploadUrl = "https://storage/upload-url";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((uploadUrl, expiresAt));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().NotBeEmpty();
        result.UploadUrl.Should().Be(uploadUrl);
        result.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));

        _mockRepository.Verify(
            x => x.AddAsync(It.Is<Document>(d => 
                d.ProviderId == providerId &&
                d.DocumentType == EDocumentType.IdentityDocument &&
                d.FileName == "test.pdf" &&
                d.Status == EDocumentStatus.Uploaded),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithProofOfResidence_ShouldCreateCorrectDocumentType()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Guid.NewGuid(),
            "ProofOfResidence",
            "proof.pdf",
            "application/pdf",
            51200);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("upload", DateTime.UtcNow.AddHours(1)));

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.AddAsync(It.Is<Document>(d => d.DocumentType == EDocumentType.ProofOfResidence), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldGenerateBlobStorageUrl()
    {
        // Arrange
        var fileName = "criminal-record.pdf";
        var command = new UploadDocumentCommand(
            Guid.NewGuid(),
            "CriminalRecord",
            fileName,
            "application/pdf",
            204800);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(("upload-url", DateTime.UtcNow.AddHours(1)));

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockBlobStorage.Verify(
            x => x.GenerateUploadUrlAsync(
                It.IsAny<string>(),
                "application/pdf",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
