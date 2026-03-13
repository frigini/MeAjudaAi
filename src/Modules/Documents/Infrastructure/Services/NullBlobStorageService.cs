using MeAjudaAi.Modules.Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

/// <summary>
/// Implementação nula (no-op) de <see cref="IBlobStorageService"/> usada quando
/// o Azure Blob Storage não está configurado (ex.: ambiente de desenvolvimento local,
/// geração de spec OpenAPI via Swagger CLI).
/// Todos os métodos lançam <see cref="NotSupportedException"/> pois operações de
/// blob não funcionam sem as credenciais do Azure.
/// </summary>
internal sealed class NullBlobStorageService : IBlobStorageService
{
    private readonly ILogger<NullBlobStorageService> _logger;

    public NullBlobStorageService(ILogger<NullBlobStorageService> logger)
    {
        _logger = logger;
    }

    public Task<(string UploadUrl, DateTime ExpiresAt)> GenerateUploadUrlAsync(
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("BlobStorageService not configured. Upload URL generation is unavailable.");
        throw new NotSupportedException(
            "Azure Blob Storage is not configured. Set 'Azure:Storage:ConnectionString' to enable file uploads.");
    }

    public Task<(string DownloadUrl, DateTime ExpiresAt)> GenerateDownloadUrlAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("BlobStorageService not configured. Download URL generation is unavailable.");
        throw new NotSupportedException(
            "Azure Blob Storage is not configured. Set 'Azure:Storage:ConnectionString' to enable file downloads.");
    }

    public Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("BlobStorageService not configured. Blob existence check is unavailable.");
        return Task.FromResult(false);
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("BlobStorageService not configured. Blob deletion is unavailable.");
        return Task.CompletedTask;
    }
}
