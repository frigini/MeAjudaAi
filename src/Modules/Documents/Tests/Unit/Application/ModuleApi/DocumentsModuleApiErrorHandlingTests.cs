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
/// Testes de error handling e edge cases para DocumentsModuleApi
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Component", "ModuleApi")]
public class DocumentsModuleApiErrorHandlingTests
{
    private readonly Mock<IQueryHandler<GetDocumentStatusQuery, DocumentDto?>> _getDocumentStatusHandlerMock;
    private readonly Mock<IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>> _getProviderDocumentsHandlerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<DocumentsModuleApi>> _loggerMock;
    private readonly DocumentsModuleApi _sut;

    public DocumentsModuleApiErrorHandlingTests()
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

    #region GetDocumentByIdAsync Error Handling

    [Fact]
    public async Task GetDocumentByIdAsync_WhenHandlerThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _getDocumentStatusHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentStatusQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.GetDocumentByIdAsync(documentId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("DOCUMENTS_GET_FAILED");
    }

    [Fact]
    public async Task GetDocumentByIdAsync_WhenOperationCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _getDocumentStatusHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentStatusQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = async () => await _sut.GetDocumentByIdAsync(documentId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region GetProviderDocumentsAsync Error Handling

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
        result.Error.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
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

    #endregion

    #region HasVerifiedDocumentsAsync Error Handling

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
        result.Error.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
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

    #endregion

    #region HasRequiredDocumentsAsync Error Handling

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
        result.Error.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
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

    #endregion

    #region GetDocumentStatusCountAsync Error Handling

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
        result.Error.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
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

    #endregion

    #region HasPendingDocumentsAsync Error Handling

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
        result.Error.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    #endregion

    #region HasRejectedDocumentsAsync Error Handling

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
        result.Error.Message.Should().Be("DOCUMENTS_PROVIDER_GET_FAILED");
    }

    #endregion

    #region IsAvailableAsync Error Handling

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

        _getDocumentStatusHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<GetDocumentStatusQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(HealthCheckService)))
            .Throws(new NullReferenceException("Unexpected error"));

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

    #endregion

    #region Helper Methods

    private static DocumentDto CreateDocumentDto(
        Guid id,
        EDocumentStatus status,
        Guid providerId,
        EDocumentType documentType)
    {
        return new DocumentDto(
            id,
            providerId,
            documentType,
            "test-document.pdf",
            "https://storage.example.com/test-document.pdf",
            status,
            DateTime.UtcNow.AddDays(-5),
            status == EDocumentStatus.Verified || status == EDocumentStatus.Rejected ? DateTime.UtcNow : null,
            status == EDocumentStatus.Rejected ? "Invalid document" : null,
            null);
    }

    #endregion
}
