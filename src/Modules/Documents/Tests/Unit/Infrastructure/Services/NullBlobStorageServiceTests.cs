using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
public sealed class NullBlobStorageServiceTests
{
    private readonly Mock<ILogger<NullBlobStorageService>> _loggerMock;
    private readonly NullBlobStorageService _service;

    public NullBlobStorageServiceTests()
    {
        _loggerMock = new Mock<ILogger<NullBlobStorageService>>();
        _service = new NullBlobStorageService(_loggerMock.Object);
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_ShouldThrowNotSupportedException()
    {
        var act = () => _service.GenerateUploadUrlAsync("blob-name", "application/pdf");

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Azure Blob Storage is not configured*");
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_ShouldLogWarning()
    {
        try { await _service.GenerateUploadUrlAsync("blob-name", "application/pdf"); } catch { }

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_ShouldThrowNotSupportedException()
    {
        var act = () => _service.GenerateDownloadUrlAsync("blob-name");

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*Azure Blob Storage is not configured*");
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_ShouldLogWarning()
    {
        try { await _service.GenerateDownloadUrlAsync("blob-name"); } catch { }

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        var result = await _service.ExistsAsync("blob-name");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ShouldLogWarning()
    {
        await _service.ExistsAsync("blob-name");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCompleteWithoutException()
    {
        var act = () => _service.DeleteAsync("blob-name");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldLogWarning()
    {
        await _service.DeleteAsync("blob-name");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}