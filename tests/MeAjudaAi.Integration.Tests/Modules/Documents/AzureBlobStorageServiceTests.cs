using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

public class AzureBlobStorageServiceTests
{
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly Mock<BlobContainerClient> _containerClientMock;
    private readonly Mock<BlobClient> _blobClientMock;
    private readonly Mock<ILogger<AzureBlobStorageService>> _loggerMock;

    public AzureBlobStorageServiceTests()
    {
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _containerClientMock = new Mock<BlobContainerClient>();
        _blobClientMock = new Mock<BlobClient>();
        _loggerMock = new Mock<ILogger<AzureBlobStorageService>>();

        _blobServiceClientMock
            .Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_containerClientMock.Object);

        _containerClientMock
            .Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);

        // Default setup for ExistsAsync to prevent null reference
        var existsResponse = Response.FromValue(true, Mock.Of<Response>());
        _blobClientMock
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsResponse);
    }

    [Fact]
    public async Task EnsureContainerExists_WhenContainerDoesNotExist_ShouldCreateContainer()
    {
        // Arrange
        var response = Response.FromValue(
            BlobsModelFactory.BlobContainerInfo(default, default),
            Mock.Of<Response>());

        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.ExistsAsync("test.pdf");

        // Assert
        _containerClientMock.Verify(
            x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnsureContainerExists_WhenContainerAlreadyExists_ShouldNotCreateAgain()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null); // Container already exists

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        await service.ExistsAsync("test1.pdf");
        await service.ExistsAsync("test2.pdf"); // Second call

        // Assert
        _containerClientMock.Verify(
            x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once); // Should only create once, not twice
    }

    [Fact]
    public async Task EnsureContainerExists_WhenCalledConcurrently_ShouldOnlyCreateOnce()
    {
        // Arrange
        var response = Response.FromValue(
            BlobsModelFactory.BlobContainerInfo(default, default),
            Mock.Of<Response>());

        var creationDelayTcs = new TaskCompletionSource<Response<BlobContainerInfo>>();

        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(100); // Simulate slow creation
                return response;
            });

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act - Call concurrently 10 times
        var tasks = Enumerable.Range(0, 10)
            .Select(i => Task.Run(() => service.ExistsAsync($"test{i}.pdf")))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert - Should only create container once despite concurrent calls
        _containerClientMock.Verify(
            x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EnsureContainerExists_WhenCreationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(500, "Internal Server Error"));

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await service.ExistsAsync("test.pdf");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to ensure blob container 'documents' exists");
    }

    [Fact]
    public async Task ExistsAsync_WhenBlobExists_ShouldReturnTrue()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null);

        var existsResponse = Response.FromValue(true, Mock.Of<Response>());
        _blobClientMock
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsResponse);

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.ExistsAsync("existing-blob.pdf");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenBlobDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null);

        var existsResponse = Response.FromValue(false, Mock.Of<Response>());
        _blobClientMock
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsResponse);

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.ExistsAsync("nonexistent-blob.pdf");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_When404Exception_ShouldReturnFalse()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null);

        _blobClientMock
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(404, "Not Found"));

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        var result = await service.ExistsAsync("blob.pdf");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenBlobExists_ShouldDeleteSuccessfully()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null);

        _blobClientMock
            .Setup(x => x.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        await service.DeleteAsync("blob-to-delete.pdf");

        // Assert
        _blobClientMock.Verify(
            x => x.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateUploadUrlAsync_WhenCannotGenerateSas_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null);

        _containerClientMock
            .Setup(x => x.CanGenerateSasUri)
            .Returns(false);

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await service.GenerateUploadUrlAsync("test.pdf", "application/pdf");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Service not configured to generate SAS tokens");
    }

    [Fact]
    public async Task GenerateDownloadUrlAsync_WhenCannotGenerateSas_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _containerClientMock
            .Setup(x => x.CreateIfNotExistsAsync(
                PublicAccessType.None,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Response<BlobContainerInfo>?)null);

        _containerClientMock
            .Setup(x => x.CanGenerateSasUri)
            .Returns(false);

        var service = new AzureBlobStorageService(_blobServiceClientMock.Object, _loggerMock.Object);

        // Act
        var act = async () => await service.GenerateDownloadUrlAsync("test.pdf");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Service not configured to generate SAS tokens");
    }
}
