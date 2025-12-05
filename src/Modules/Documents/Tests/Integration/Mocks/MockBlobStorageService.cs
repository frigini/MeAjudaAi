using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Mocks;

public class MockBlobStorageService : IBlobStorageService
{
    private readonly Dictionary<string, bool> _storedBlobs = new();

    public Task<(string UploadUrl, DateTime ExpiresAt)> GenerateUploadUrlAsync(
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uploadUrl = $"https://mock-storage.blob.core.windows.net/documents/{blobName}?sas=mock-upload-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        
        // Simulate storage
        _storedBlobs[blobName] = true;
        
        return Task.FromResult((uploadUrl, expiresAt));
    }

    public Task<(string DownloadUrl, DateTime ExpiresAt)> GenerateDownloadUrlAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var downloadUrl = $"https://mock-storage.blob.core.windows.net/documents/{blobName}?sas=mock-download-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        
        return Task.FromResult((downloadUrl, expiresAt));
    }

    public Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storedBlobs.ContainsKey(blobName));
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        _storedBlobs.Remove(blobName);
        return Task.CompletedTask;
    }
}
