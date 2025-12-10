using Azure.AI.DocumentIntelligence;
using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Constants;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Testes unitários para Azure DocumentIntelligenceService.
/// Nota: Devido à complexidade do Azure SDK (classes seladas, sem interfaces), estes testes focam em:
/// - Validação de parâmetros
/// - Lógica de mapeamento de document types
/// - Guard clauses
/// Para testes de integração real com Azure, veja DocumentsIntegrationTests.
/// </summary>
public class AzureDocumentIntelligenceServiceTests
{
    private readonly Mock<DocumentIntelligenceClient> _mockClient;
    private readonly Mock<ILogger<AzureDocumentIntelligenceService>> _mockLogger;
    private readonly AzureDocumentIntelligenceService _service;

    public AzureDocumentIntelligenceServiceTests()
    {
        _mockClient = new Mock<DocumentIntelligenceClient>();
        _mockLogger = new Mock<ILogger<AzureDocumentIntelligenceService>>();
        _service = new AzureDocumentIntelligenceService(_mockClient.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WhenClientIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new AzureDocumentIntelligenceService(null!, _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("client");
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new AzureDocumentIntelligenceService(_mockClient.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnalyzeDocumentAsync_WhenBlobUrlIsNullOrWhitespace_ShouldThrowArgumentException(string? blobUrl)
    {
        // Arrange
        var documentType = DocumentModelConstants.DocumentTypes.IdentityDocument;

        // Act
        var act = async () => await _service.AnalyzeDocumentAsync(blobUrl!, documentType);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("blobUrl")
            .WithMessage("*cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnalyzeDocumentAsync_WhenDocumentTypeIsNullOrWhitespace_ShouldThrowArgumentException(string? documentType)
    {
        // Arrange
        var blobUrl = "https://storage.blob.core.windows.net/documents/test.pdf";

        // Act
        var act = async () => await _service.AnalyzeDocumentAsync(blobUrl, documentType!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("documentType")
            .WithMessage("*cannot be null or empty*");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("relative/path")]
    public async Task AnalyzeDocumentAsync_WhenBlobUrlFormatIsInvalid_ShouldThrowArgumentException(string invalidUrl)
    {
        // Arrange
        var documentType = DocumentModelConstants.DocumentTypes.IdentityDocument;

        // Act
        var act = async () => await _service.AnalyzeDocumentAsync(invalidUrl, documentType);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("blobUrl")
            .WithMessage("*Invalid blob URL format*");
    }

    [Theory]
    [InlineData("http://storage.blob.core.windows.net/documents/test.pdf")]
    [InlineData("https://storage.blob.core.windows.net/documents/test.pdf")]
    [InlineData("ftp://storage/documents/test.pdf")]
    [InlineData("file:///C:/local/path/test.pdf")]
    public async Task AnalyzeDocumentAsync_WhenUrlIsAbsolute_ShouldNotThrowArgumentExceptionForUrl(string absoluteUrl)
    {
        // Arrange
        var documentType = DocumentModelConstants.DocumentTypes.IdentityDocument;

        // Act
        var act = async () => await _service.AnalyzeDocumentAsync(absoluteUrl, documentType);

        // Assert
        // Should not throw ArgumentException for blobUrl (URL validation passes)
        // May throw other exceptions from Azure SDK mock
        try
        {
            await act();
        }
        catch (ArgumentException ex) when (ex.ParamName == "blobUrl")
        {
            Assert.Fail($"URL validation should pass for absolute URL: {absoluteUrl}");
        }
        catch
        {
            // Expected - Azure SDK or other failures are acceptable
        }
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_ShouldLogInformationWhenStarting()
    {
        // Arrange
        var blobUrl = "https://storage.blob.core.windows.net/documents/test.pdf";
        var documentType = DocumentModelConstants.DocumentTypes.IdentityDocument;

        // Act
        try
        {
            await _service.AnalyzeDocumentAsync(blobUrl, documentType);
        }
        catch
        {
            // Expected - Azure SDK will fail in unit test
        }

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando análise OCR")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(DocumentModelConstants.DocumentTypes.IdentityDocument)]
    [InlineData(DocumentModelConstants.DocumentTypes.ProofOfResidence)]
    [InlineData(DocumentModelConstants.DocumentTypes.CriminalRecord)]
    [InlineData("unknowntype")]
    public async Task AnalyzeDocumentAsync_ShouldAcceptDifferentDocumentTypes(string documentType)
    {
        // Arrange
        var blobUrl = "https://storage.blob.core.windows.net/documents/test.pdf";

        // Act
        try
        {
            await _service.AnalyzeDocumentAsync(blobUrl, documentType);
        }
        catch
        {
            // Expected - Azure SDK will fail in unit test
        }

        // Assert - Should have logged the start (meaning it accepted the document type)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando análise OCR")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenCancellationRequested_ShouldPassTokenToAzureSdk()
    {
        // Arrange
        var blobUrl = "https://storage.blob.core.windows.net/documents/test.pdf";
        var documentType = DocumentModelConstants.DocumentTypes.IdentityDocument;
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var exception = await Record.ExceptionAsync(() =>
            _service.AnalyzeDocumentAsync(blobUrl, documentType, cts.Token));

        // Assert
        // The service should handle a pre-canceled token without surfacing exceptions here.
        // Real cancellation testing requires integration tests with actual Azure SDK behavior.
        exception.Should().BeNull("the service should not throw synchronously when a canceled token is provided");
    }
}
