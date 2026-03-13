using FluentAssertions;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
public class NullDocumentIntelligenceServiceTests
{
    private readonly Mock<ILogger<NullDocumentIntelligenceService>> _mockLogger;
    private readonly NullDocumentIntelligenceService _service;

    public NullDocumentIntelligenceServiceTests()
    {
        _mockLogger = new Mock<ILogger<NullDocumentIntelligenceService>>();
        _service = new NullDocumentIntelligenceService(_mockLogger.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnalyzeDocumentAsync_WhenBlobUrlIsNullOrWhitespace_ShouldThrowArgumentException(string? blobUrl)
    {
        // Act
        var act = async () => await _service.AnalyzeDocumentAsync(blobUrl!, "IdentityDocument");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("blobUrl")
            .WithMessage("Blob URL cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnalyzeDocumentAsync_WhenDocumentTypeIsNullOrWhitespace_ShouldThrowArgumentException(string? documentType)
    {
        // Act
        var act = async () => await _service.AnalyzeDocumentAsync("https://example.com/blob", documentType!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("documentType")
            .WithMessage("Document type cannot be null or empty*");
    }

    [Fact]
    public async Task AnalyzeDocumentAsync_WhenUrlIsInvalidFormat_ShouldThrowArgumentException()
    {
        // Act
        var act = async () => await _service.AnalyzeDocumentAsync("invalid-url", "IdentityDocument");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("blobUrl")
            .WithMessage("Invalid blob URL format*");
    }


    [Fact]
    public async Task AnalyzeDocumentAsync_WhenValidRequest_ShouldReturnFailureResultWithPortugueseMessage()
    {
        // Arrange
        var blobUrl = "https://storage.blob.core.windows.net/container/doc.pdf";
        var documentType = "IdentityDocument";

        // Act
        var result = await _service.AnalyzeDocumentAsync(blobUrl, documentType);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Não foi possível processar o documento no momento, tente novamente mais tarde.");
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("DocumentIntelligenceService not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
