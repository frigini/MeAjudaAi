using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
public class NullBlobStorageServiceTests
{
    private readonly Mock<ILogger<NullBlobStorageService>> _mockLogger;
    private readonly NullBlobStorageService _service;

    public NullBlobStorageServiceTests()
    {
        _mockLogger = new Mock<ILogger<NullBlobStorageService>>();
        _service = new NullBlobStorageService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_ShouldThrowNotSupportedException()
    {
        // Act
        var act = async () => await _service.GenerateUploadUrlAsync("blob-name", "application/pdf");

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Azure Blob Storage is not configured*");

        VerifyWarningLogged("Upload URL generation is unavailable");
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_ShouldThrowNotSupportedException()
    {
        // Act
        var act = async () => await _service.GenerateDownloadUrlAsync("blob-name");

        // Assert
        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Azure Blob Storage is not configured*");

        VerifyWarningLogged("Download URL generation is unavailable");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        // Act
        var result = await _service.ExistsAsync("blob-name");

        // Assert
        result.Should().BeFalse();
        VerifyWarningLogged("Blob existence check is unavailable");
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow()
    {
        // Act
        var act = async () => await _service.DeleteAsync("blob-name");

        // Assert
        await act.Should().NotThrowAsync();
        VerifyWarningLogged("Blob deletion is unavailable");
    }

    private void VerifyWarningLogged(string expectedMessagePart)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessagePart)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
