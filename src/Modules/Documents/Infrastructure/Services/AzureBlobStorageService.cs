using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

public class AzureBlobStorageService(BlobServiceClient blobServiceClient, ILogger<AzureBlobStorageService> logger) : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient = blobServiceClient.GetBlobContainerClient("documents");
    private readonly ILogger<AzureBlobStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<(string UploadUrl, DateTime ExpiresAt)> GenerateUploadUrlAsync(
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var expiresAt = DateTime.UtcNow.AddHours(1); // SAS válido por 1 hora

            // Verifica se temos permissões para gerar SAS
            if (!_containerClient.CanGenerateSasUri)
            {
                _logger.LogWarning("BlobContainerClient não pode gerar SAS URIs. Verifique as credenciais.");
                throw new InvalidOperationException("Serviço não configurado para gerar SAS tokens");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b", // blob
                ExpiresOn = expiresAt
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation("SAS token de upload gerado para blob {BlobName}, expira em {ExpiresAt}",
                blobName, expiresAt);

            return (sasUri.ToString(), expiresAt);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Erro ao gerar SAS token de upload para blob {BlobName}", blobName);
            throw;
        }
    }

    public async Task<(string DownloadUrl, DateTime ExpiresAt)> GenerateDownloadUrlAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var expiresAt = DateTime.UtcNow.AddHours(24); // Download válido por 24 horas

            if (!_containerClient.CanGenerateSasUri)
            {
                _logger.LogWarning("BlobContainerClient não pode gerar SAS URIs.");
                throw new InvalidOperationException("Serviço não configurado para gerar SAS tokens");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerClient.Name,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = expiresAt
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation("SAS token de download gerado para blob {BlobName}, expira em {ExpiresAt}",
                blobName, expiresAt);

            return (sasUri.ToString(), expiresAt);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Erro ao gerar SAS token de download para blob {BlobName}", blobName);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            return await blobClient.ExistsAsync(cancellationToken);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do blob {BlobName}", blobName);
            return false;
        }
    }

    public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("Blob {BlobName} deletado", blobName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Erro ao deletar blob {BlobName}", blobName);
            throw;
        }
    }
}
