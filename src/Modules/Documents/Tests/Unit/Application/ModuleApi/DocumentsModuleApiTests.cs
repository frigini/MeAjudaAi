using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.ModuleApi;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.ModuleApi;

/// <summary>
/// Testes unitários para DocumentsModuleApi
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Component", "ModuleApi")]
public class DocumentsModuleApiTests
{
    private readonly Mock<IQueryHandler<GetDocumentStatusQuery, DocumentDto?>> _getDocumentStatusHandlerMock;
    private readonly Mock<IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>> _getProviderDocumentsHandlerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<DocumentsModuleApi>> _loggerMock;
    private readonly DocumentsModuleApi _sut;

    public DocumentsModuleApiTests()
    {
        _getDocumentStatusHandlerMock = new Mock<IQueryHandler<GetDocumentStatusQuery, DocumentDto?>>();
        _getProviderDocumentsHandlerMock = new Mock<IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<DocumentsModuleApi>>();

        _sut = new DocumentsModuleApi(
            _getDocumentStatusHandlerMock.Object,
            _getProviderDocumentsHandlerMock.Object,
            _serviceProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturnDocuments()
    {
        // Act
        var result = _sut.ModuleName;

        // Assert
        result.Should().Be("Documents");
    }

    [Fact]
    public void ApiVersion_ShouldReturn1Point0()
    {
        // Act
        var result = _sut.ApiVersion;

        // Assert
        result.Should().Be("1.0");
    }

    [Fact]
    public async Task GetDocumentByIdAsync_WithExistingDocument_ShouldReturnDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentDto = CreateDocumentDto(documentId, EDocumentStatus.Verified);

        _getDocumentStatusHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentDto);

        // Act
        var result = await _sut.GetDocumentByIdAsync(documentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(documentId);
        result.Value.Status.Should().Be(EDocumentStatus.Verified.ToString());
    }

    [Fact]
    public async Task GetDocumentByIdAsync_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _getDocumentStatusHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentDto?)null);

        // Act
        var result = await _sut.GetDocumentByIdAsync(documentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderDocumentsAsync_WithDocuments_ShouldReturnList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.PendingVerification, providerId)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.GetProviderDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(d => d.ProviderId.Should().Be(providerId));
    }

    [Fact]
    public async Task HasVerifiedDocumentsAsync_WithVerifiedDocuments_ShouldReturnTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.PendingVerification, providerId)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.HasVerifiedDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasVerifiedDocumentsAsync_WithNoVerifiedDocuments_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.PendingVerification, providerId),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Rejected, providerId)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.HasVerifiedDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task HasRequiredDocumentsAsync_WithIdentityAndProofOfResidence_ShouldReturnTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId, EDocumentType.IdentityDocument),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId, EDocumentType.ProofOfResidence)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.HasRequiredDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasRequiredDocumentsAsync_WithOnlyIdentity_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId, EDocumentType.IdentityDocument)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.HasRequiredDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetDocumentStatusCountAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.PendingVerification, providerId),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Rejected, providerId),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Uploaded, providerId)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.GetDocumentStatusCountAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Total.Should().Be(5);
        result.Value.Verified.Should().Be(2);
        result.Value.Pending.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
        result.Value.Uploading.Should().Be(1);
    }

    [Fact]
    public async Task HasPendingDocumentsAsync_WithPendingDocuments_ShouldReturnTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.PendingVerification, providerId)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.HasPendingDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasRejectedDocumentsAsync_WithRejectedDocuments_ShouldReturnTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Rejected, providerId)
        };

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _sut.HasRejectedDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WithoutHealthCheckService_ShouldPerformBasicOperationsTest()
    {
        // Arrange
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns((HealthCheckService?)null);

        _getDocumentStatusHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DocumentDto?)null);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    private static DocumentDto CreateDocumentDto(
        Guid id,
        EDocumentStatus status,
        Guid? providerId = null,
        EDocumentType documentType = EDocumentType.IdentityDocument)
    {
        return new DocumentDto(
            id,
            providerId ?? Guid.NewGuid(),
            documentType,
            "test-document.pdf",
            "https://storage.example.com/test-document.pdf",
            status,
            DateTime.UtcNow.AddDays(-5),
            // VerifiedAt is set when document is Verified OR Rejected (domain behavior)
            status == EDocumentStatus.Verified || status == EDocumentStatus.Rejected ? DateTime.UtcNow : null,
            status == EDocumentStatus.Rejected ? "Invalid document" : null,
            null);
    }
}
