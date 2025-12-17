using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Services;

public class AzureBlobStorageServiceTests
{
    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
    private readonly Mock<BlobContainerClient> _mockContainerClient;
    private readonly Mock<BlobClient> _mockBlobClient;
    private readonly Mock<ILogger<AzureBlobStorageService>> _mockLogger;
    private readonly AzureBlobStorageService _service;

    public AzureBlobStorageServiceTests()
    {
        _mockBlobServiceClient = new Mock<BlobServiceClient>();
        _mockContainerClient = new Mock<BlobContainerClient>();
        _mockBlobClient = new Mock<BlobClient>();
        _mockLogger = new Mock<ILogger<AzureBlobStorageService>>();

        // Setup: BlobServiceClient retorna ContainerClient
        _mockBlobServiceClient
            .Setup(x => x.GetBlobContainerClient("documents"))
            .Returns(_mockContainerClient.Object);

        // Setup: ContainerClient retorna BlobClient
        _mockContainerClient
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(_mockBlobClient.Object);

        _service = new AzureBlobStorageService(_mockBlobServiceClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_ShouldReturnValidSasUrl()
    {
        // Preparação
        var blobName = "test-document.pdf";
        var contentType = "application/pdf";
        var expectedUrl = new Uri("https://storage.blob.core.windows.net/documents/test-document.pdf?sv=2021-06-08&st=2024-01-01T00%3A00%3A00Z&se=2024-01-01T01%3A00%3A00Z&sr=b&sp=cw&sig=signature");

        _mockContainerClient
            .Setup(x => x.CanGenerateSasUri)
            .Returns(true);

        _mockBlobClient
            .Setup(x => x.GenerateSasUri(It.IsAny<BlobSasBuilder>()))
            .Returns(expectedUrl);

        // Ação
        var (uploadUrl, expiresAt) = await _service.GenerateUploadUrlAsync(blobName, contentType);

        // Verificação
        uploadUrl.Should().NotBeNullOrEmpty();
        uploadUrl.Should().Be(expectedUrl.ToString());
        expiresAt.Should().BeAfter(DateTime.UtcNow);
        expiresAt.Should().BeOnOrBefore(DateTime.UtcNow.AddHours(1).AddMinutes(1)); // Tolerate 1 minute variance
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_WhenCannotGenerateSasUri_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var blobName = "test-document.pdf";
        var contentType = "application/pdf";

        _mockContainerClient
            .Setup(x => x.CanGenerateSasUri)
            .Returns(false);

        // Ação
        var act = () => _service.GenerateUploadUrlAsync(blobName, contentType);

        // Verificação
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Serviço não configurado para gerar SAS tokens");
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_WhenRequestFails_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var blobName = "test-document.pdf";
        var contentType = "application/pdf";

        _mockContainerClient
            .Setup(x => x.CanGenerateSasUri)
            .Returns(true);

        _mockBlobClient
            .Setup(x => x.GenerateSasUri(It.IsAny<BlobSasBuilder>()))
            .Throws(new RequestFailedException("Azure Blob Storage error"));

        // Ação
        var act = () => _service.GenerateUploadUrlAsync(blobName, contentType);

        // Verificação
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.And.InnerException.Should().BeOfType<RequestFailedException>();
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_ShouldReturnValidSasUrl()
    {
        // Preparação
        var blobName = "test-document.pdf";
        var expectedUrl = new Uri("https://storage.blob.core.windows.net/documents/test-document.pdf?sv=2021-06-08&st=2024-01-01T00%3A00%3A00Z&se=2024-01-02T00%3A00%3A00Z&sr=b&sp=r&sig=signature");

        _mockContainerClient
            .Setup(x => x.CanGenerateSasUri)
            .Returns(true);

        _mockBlobClient
            .Setup(x => x.GenerateSasUri(It.IsAny<BlobSasBuilder>()))
            .Returns(expectedUrl);

        // Ação
        var (downloadUrl, expiresAt) = await _service.GenerateDownloadUrlAsync(blobName);

        // Verificação
        downloadUrl.Should().NotBeNullOrEmpty();
        downloadUrl.Should().Be(expectedUrl.ToString());
        expiresAt.Should().BeAfter(DateTime.UtcNow);
        expiresAt.Should().BeOnOrBefore(DateTime.UtcNow.AddHours(24).AddMinutes(1));
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_WhenCannotGenerateSasUri_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var blobName = "test-document.pdf";

        _mockContainerClient
            .Setup(x => x.CanGenerateSasUri)
            .Returns(false);

        // Ação
        var act = () => _service.GenerateDownloadUrlAsync(blobName);

        // Verificação
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Serviço não configurado para gerar SAS tokens");
    }

    [Fact]
    public async Task ExistsAsync_WhenBlobExists_ShouldReturnTrue()
    {
        // Preparação
        var blobName = "existing-document.pdf";
        var response = Response.FromValue(true, Mock.Of<Response>());

        _mockBlobClient
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Ação
        var result = await _service.ExistsAsync(blobName);

        // Verificação
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenBlobDoesNotExist_ShouldReturnFalse()
    {
        // Preparação
        var blobName = "non-existent-document.pdf";
        var response = Response.FromValue(false, Mock.Of<Response>());

        _mockBlobClient
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Ação
        var result = await _service.ExistsAsync(blobName);

        // Verificação
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenRequestFails_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var blobName = "test-document.pdf";

        _mockBlobClient
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Azure error"));

        // Ação
        var act = () => _service.ExistsAsync(blobName);

        // Verificação
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.And.InnerException.Should().BeOfType<RequestFailedException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenBlobExists_ShouldDeleteSuccessfully()
    {
        // Preparação
        var blobName = "document-to-delete.pdf";
        var response = Response.FromValue(true, Mock.Of<Response>());

        _mockBlobClient
            .Setup(x => x.DeleteIfExistsAsync(
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Ação
        await _service.DeleteAsync(blobName);

        // Verificação
        _mockBlobClient.Verify(
            x => x.DeleteIfExistsAsync(
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRequestFails_ShouldThrowInvalidOperationException()
    {
        // Preparação
        var blobName = "document-to-delete.pdf";

        _mockBlobClient
            .Setup(x => x.DeleteIfExistsAsync(
                It.IsAny<DeleteSnapshotsOption>(),
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Delete failed"));

        // Ação
        var act = () => _service.DeleteAsync(blobName);

        // Verificação
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.And.InnerException.Should().BeOfType<RequestFailedException>();
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Ação
        var act = () => new AzureBlobStorageService(_mockBlobServiceClient.Object, null!);

        // Verificação
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}
