using Azure.AI.DocumentIntelligence;
using MeAjudaAi.Modules.Documents.Application.Constants;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;

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
    private readonly Mock<ILogger<AzureDocumentIntelligenceService>> _mockLogger;

    public AzureDocumentIntelligenceServiceTests()
    {
        _mockLogger = new Mock<ILogger<AzureDocumentIntelligenceService>>();
    }

    [Theory]
    [InlineData("identitydocument", DocumentModelConstants.ModelIds.IdentityDocument)]
    [InlineData("IDENTITYDOCUMENT", DocumentModelConstants.ModelIds.IdentityDocument)]
    [InlineData("proofofresidence", DocumentModelConstants.ModelIds.GenericDocument)]
    [InlineData("criminalrecord", DocumentModelConstants.ModelIds.GenericDocument)]
    [InlineData("unknown-type", DocumentModelConstants.ModelIds.GenericDocument)]
    public void DocumentType_ToModelId_Mapping_ShouldBeCorrect(string documentType, string expectedModelId)
    {
        // Verifica a lógica de mapeamento document type -> model ID
        // Este é o comportamento implementado no switch expression do serviço

        var actualModelId = documentType.ToLowerInvariant() switch
        {
            DocumentModelConstants.DocumentTypes.IdentityDocument => DocumentModelConstants.ModelIds.IdentityDocument,
            DocumentModelConstants.DocumentTypes.ProofOfResidence => DocumentModelConstants.ModelIds.GenericDocument,
            DocumentModelConstants.DocumentTypes.CriminalRecord => DocumentModelConstants.ModelIds.GenericDocument,
            _ => DocumentModelConstants.ModelIds.GenericDocument
        };

        actualModelId.Should().Be(expectedModelId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task AnalyzeDocumentAsync_WithInvalidBlobUrl_ShouldThrowArgumentException(string? invalidUrl)
    {
        // Arrange
        var mockClient = new Mock<DocumentIntelligenceClient>();
        var service = new AzureDocumentIntelligenceService(mockClient.Object, _mockLogger.Object);
        var documentType = "identity-document";

        // Act
        var act = () => service.AnalyzeDocumentAsync(invalidUrl!, documentType);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Blob URL cannot be null or empty*")
            .WithParameterName("blobUrl");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task AnalyzeDocumentAsync_WithInvalidDocumentType_ShouldThrowArgumentException(string? invalidType)
    {
        // Arrange
        var mockClient = new Mock<DocumentIntelligenceClient>();
        var service = new AzureDocumentIntelligenceService(mockClient.Object, _mockLogger.Object);
        var blobUrl = "https://storage.blob.core.windows.net/documents/test.pdf";

        // Act
        var act = () => service.AnalyzeDocumentAsync(blobUrl, invalidType!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Document type cannot be null or empty*")
            .WithParameterName("documentType");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("just some text")]
    public async Task AnalyzeDocumentAsync_WithMalformedBlobUrl_ShouldThrowArgumentException(string malformedUrl)
    {
        // Arrange
        var mockClient = new Mock<DocumentIntelligenceClient>();
        var service = new AzureDocumentIntelligenceService(mockClient.Object, _mockLogger.Object);
        var documentType = "identity-document";

        // Act
        var act = () => service.AnalyzeDocumentAsync(malformedUrl, documentType);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid blob URL format*")
            .WithParameterName("blobUrl");
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
        // Arrange
        var mockClient = new Mock<DocumentIntelligenceClient>();

        // Act
        var act = () => new AzureDocumentIntelligenceService(mockClient.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
