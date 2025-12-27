using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient, ILogger<AzureBlobStorageService> logger) : IBlobStorageService, IAsyncDisposable
{
    private const string ContainerName = "documents";
    private readonly BlobServiceClient _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    private readonly ILogger<AzureBlobStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private volatile bool _containerInitialized;

    /// <summary>
    /// Garante que o container "documents" existe antes de usar.
    /// Thread-safe e executa apenas uma vez.
    /// </summary>
    private async Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        if (_containerInitialized)
            return;

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_containerInitialized)
                return;

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var response = await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            
            if (response != null)
            {
                _logger.LogInformation("Blob container '{ContainerName}' created successfully", ContainerName);
            }
            else
            {
                _logger.LogDebug("Blob container '{ContainerName}' already exists", ContainerName);
            }

            _containerInitialized = true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error creating container '{ContainerName}'", ContainerName);
            throw new InvalidOperationException($"Failed to ensure blob container '{ContainerName}' exists", ex);
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<(string UploadUrl, DateTime ExpiresAt)> GenerateUploadUrlAsync(
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await EnsureContainerExistsAsync(cancellationToken);
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        
        try
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            var expiresAt = DateTime.UtcNow.AddHours(1); // SAS v├ílido por 1 hora

            // Verifica se temos permiss├╡es para gerar SAS
            if (!containerClient.CanGenerateSasUri)
            {
                _logger.LogWarning("BlobContainerClient cannot generate SAS URIs. Verify credentials.");
                throw new InvalidOperationException("Service not configured to generate SAS tokens");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobName,
                Resource = "b", // blob
                ExpiresOn = expiresAt
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation("Upload SAS token generated for blob {BlobName}, expires at {ExpiresAt}",
                blobName, expiresAt);

            return (sasUri.ToString(), expiresAt);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error generating upload SAS token for blob {BlobName}", blobName);
            throw new InvalidOperationException(
                $"Failed to generate Azure Blob Storage SAS upload token for blob '{blobName}' (Status: {ex.Status})",
                ex);
        }
    }

    public async Task<(string DownloadUrl, DateTime ExpiresAt)> GenerateDownloadUrlAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        await EnsureContainerExistsAsync(cancellationToken);
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        
        try
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Download v├ílido por 24 horas

            if (!containerClient.CanGenerateSasUri)
            {
                _logger.LogWarning("BlobContainerClient cannot generate SAS URIs.");
                throw new InvalidOperationException("Service not configured to generate SAS tokens");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = expiresAt
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation("Download SAS token generated for blob {BlobName}, expires at {ExpiresAt}",
                blobName, expiresAt);

            return (sasUri.ToString(), expiresAt);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error generating download SAS token for blob {BlobName}", blobName);
            throw new InvalidOperationException(
                $"Failed to generate Azure Blob Storage SAS download token for blob '{blobName}' (Status: {ex.Status})",
                ex);
        }
    }

    public async Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        await EnsureContainerExistsAsync(cancellationToken);
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        
        try
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            var response = await blobClient.ExistsAsync(cancellationToken);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error checking blob existence {BlobName} (Status: {Status})", blobName, ex.Status);
            throw new InvalidOperationException(
                $"Failed to check existence of blob '{blobName}' (Status: {ex.Status})",
                ex);
        }
    }

    public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        await EnsureContainerExistsAsync(cancellationToken);
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        
        try
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("Blob {BlobName} deleted", blobName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error deleting blob {BlobName}", blobName);
            throw new InvalidOperationException(
                $"Failed to delete blob '{blobName}' from Azure Blob Storage (Status: {ex.Status})",
                ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _initializationLock.Dispose();
        await ValueTask.CompletedTask;
    }
}

