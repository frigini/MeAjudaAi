using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Documents;

/// <summary>
/// Mock de IBlobStorageService para testes de integração e E2E.
/// Simula operações de blob storage sem Azurite real.
/// </summary>
public sealed class MockBlobStorageService : IBlobStorageService
{
    private readonly HashSet<string> _existingBlobs = [];
    private readonly List<string> _deletedBlobs = [];

    public IReadOnlyList<string> DeletedBlobs => _deletedBlobs.AsReadOnly();

    public void SetBlobExists(string blobName, bool exists)
    {
        if (exists)
            _existingBlobs.Add(blobName);
        else
            _existingBlobs.Remove(blobName);
    }

    public void Reset()
    {
        _existingBlobs.Clear();
        _deletedBlobs.Clear();
    }

    public Task<(string UploadUrl, DateTime ExpiresAt)> GenerateUploadUrlAsync(
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        _existingBlobs.Add(blobName);

        var fakeUrl = $"https://mock-storage.local/documents/{blobName}?sas=mock-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        return Task.FromResult((fakeUrl, expiresAt));
    }

    public Task<(string DownloadUrl, DateTime ExpiresAt)> GenerateDownloadUrlAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        var fakeUrl = $"https://mock-storage.local/documents/{blobName}?sas=mock-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        return Task.FromResult((fakeUrl, expiresAt));
    }

    public Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_existingBlobs.Contains(blobName));
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        _existingBlobs.Remove(blobName);
        _deletedBlobs.Add(blobName);
        return Task.CompletedTask;
    }
}
