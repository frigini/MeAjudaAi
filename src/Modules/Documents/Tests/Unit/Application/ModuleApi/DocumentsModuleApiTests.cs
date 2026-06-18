using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.ModuleApi;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.ModuleApi;

/// <summary>
/// Testes unitários para DocumentsModuleApi
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Component", "ModuleApi")]
public class DocumentsModuleApiTests
{
    private readonly Mock<IQueryHandler<GetDocumentByIdQuery, DocumentDto?>> _getDocumentByIdHandlerMock;
    private readonly Mock<IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>> _getProviderDocumentsHandlerMock;
    private readonly Mock<IDocumentQueries> _documentQueriesMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<DocumentsModuleApi>> _loggerMock;
    private readonly DocumentsModuleApi _sut;

    public DocumentsModuleApiTests()
    {
        _getDocumentByIdHandlerMock = new Mock<IQueryHandler<GetDocumentByIdQuery, DocumentDto?>>();
        _getProviderDocumentsHandlerMock = new Mock<IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>>();
        _documentQueriesMock = new Mock<IDocumentQueries>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<DocumentsModuleApi>>();

        _sut = new DocumentsModuleApi(
            _getDocumentByIdHandlerMock.Object,
            _getProviderDocumentsHandlerMock.Object,
            _documentQueriesMock.Object,
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

        _getDocumentByIdHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentByIdQuery>(), It.IsAny<CancellationToken>()))
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

        _getDocumentByIdHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentByIdQuery>(), It.IsAny<CancellationToken>()))
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

        _documentQueriesMock
            .Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    #region Error Handling Tests

    [Fact]
    public async Task GetDocumentByIdAsync_WhenHandlerThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _getDocumentByIdHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.GetDocumentByIdAsync(documentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_GET_FAILED");
    }

    [Fact]
    public async Task GetDocumentByIdAsync_WhenOperationCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _getDocumentByIdHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await _sut.GetDocumentByIdAsync(documentId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetProviderDocumentsAsync_WhenHandlerThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.GetProviderDocumentsAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    [Fact]
    public async Task GetProviderDocumentsAsync_WhenOperationCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await _sut.GetProviderDocumentsAsync(providerId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task HasVerifiedDocumentsAsync_WhenGetDocumentsFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.HasVerifiedDocumentsAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    [Fact]
    public async Task HasVerifiedDocumentsAsync_WithEmptyDocumentsList_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto>());

        // Act
        var result = await _sut.HasVerifiedDocumentsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task HasRequiredDocumentsAsync_WhenGetDocumentsFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.HasRequiredDocumentsAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    [Fact]
    public async Task HasRequiredDocumentsAsync_WithVerifiedButWrongTypes_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId, EDocumentType.CriminalRecord),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.Verified, providerId, EDocumentType.Other)
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
    public async Task HasRequiredDocumentsAsync_WithRequiredButNotVerified_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<DocumentDto>
        {
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.PendingVerification, providerId, EDocumentType.IdentityDocument),
            CreateDocumentDto(Guid.NewGuid(), EDocumentStatus.PendingVerification, providerId, EDocumentType.ProofOfResidence)
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
    public async Task GetDocumentStatusCountAsync_WhenGetDocumentsFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.GetDocumentStatusCountAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    [Fact]
    public async Task GetDocumentStatusCountAsync_WithNoDocuments_ShouldReturnZeroCounts()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DocumentDto>());

        // Act
        var result = await _sut.GetDocumentStatusCountAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Total.Should().Be(0);
        result.Value.Verified.Should().Be(0);
        result.Value.Pending.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
        result.Value.Uploading.Should().Be(0);
    }

    [Fact]
    public async Task HasPendingDocumentsAsync_WhenGetDocumentsFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.HasPendingDocumentsAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    [Fact]
    public async Task HasRejectedDocumentsAsync_WhenGetDocumentsFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderDocumentsHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetProviderDocumentsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.HasRejectedDocumentsAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    [Fact]
    public async Task IsAvailableAsync_WithUnhealthyHealthCheck_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>(MockBehavior.Strict);
        var unhealthyReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new HealthReportEntry(HealthStatus.Unhealthy, "Database unavailable", TimeSpan.FromMilliseconds(100), null, null)
            },
            TimeSpan.FromMilliseconds(100));

        healthCheckServiceMock
            .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(unhealthyReport);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenBasicOperationsFail_ShouldReturnFalse()
    {
        // Arrange
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns((HealthCheckService?)null);

        _documentQueriesMock
            .Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenInvalidOperationExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Throws(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenTimeoutExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Throws(new TimeoutException("Operation timed out"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenNpgsqlExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Throws(new Npgsql.NpgsqlException("Database connection failed"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var healthCheckServiceMock = new Mock<HealthCheckService>(MockBehavior.Strict);
        healthCheckServiceMock
            .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

        // Act
        var act = async () => await _sut.IsAvailableAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyHealthCheck_AndCanConnect_ShouldReturnTrue()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>(MockBehavior.Strict);
        var healthyReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new HealthReportEntry(HealthStatus.Healthy, "OK", TimeSpan.FromMilliseconds(10), null, null)
            },
            TimeSpan.FromMilliseconds(10));

        healthCheckServiceMock
            .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthyReport);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

        _documentQueriesMock
            .Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WithHealthyHealthCheck_ButCanConnectFails_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>(MockBehavior.Strict);
        var healthyReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new HealthReportEntry(HealthStatus.Healthy, "OK", TimeSpan.FromMilliseconds(10), null, null)
            },
            TimeSpan.FromMilliseconds(10));

        healthCheckServiceMock
            .Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthyReport);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

        _documentQueriesMock
            .Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDocumentAsync_WithExistingDocument_ShouldReturnTrue()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var documentDto = CreateDocumentDto(documentId, EDocumentStatus.Verified);

        _getDocumentByIdHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentDto);

        var documentEntity = Modules.Documents.Domain.Entities.Document.Create(
            documentId, Modules.Documents.Domain.Enums.EDocumentType.IdentityDocument, "test.pdf", "blob-url");

        var repositoryMock = new Mock<Shared.Database.Abstractions.IRepository<Modules.Documents.Domain.Entities.Document, Guid>>();
        repositoryMock.Setup(x => x.TryFindAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentEntity);

        var uowMock = new Mock<Shared.Database.Abstractions.IUnitOfWork>();
        uowMock.Setup(x => x.GetRepository<Modules.Documents.Domain.Entities.Document, Guid>())
            .Returns(repositoryMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(Shared.Database.Abstractions.IUnitOfWork)))
            .Returns(uowMock.Object);

        // Act
        var result = await _sut.DeleteDocumentAsync(documentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        repositoryMock.Verify(x => x.Delete(documentEntity), Times.Once);
        uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteDocumentAsync_WithNonExistentDocument_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        _getDocumentByIdHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Modules.Documents.Application.DTOs.DocumentDto?)null);

        // Act
        var result = await _sut.DeleteDocumentAsync(documentId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDocumentAsync_WhenHandlerThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _getDocumentByIdHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.DeleteDocumentAsync(documentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("DOCUMENTS_DELETE_FAILED");
    }

    #endregion

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
            status is EDocumentStatus.Verified or EDocumentStatus.Rejected ? DateTime.UtcNow : null,
            status == EDocumentStatus.Rejected ? "Invalid document" : null,
            null);
    }
}
