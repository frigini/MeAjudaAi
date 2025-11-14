using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Services;

/// <summary>
/// Mock implementation of IBlobStorageService for testing environments
/// </summary>
public sealed class MockBlobStorageService : IBlobStorageService
{
    public Task<(string UploadUrl, DateTime ExpiresAt)> GenerateUploadUrlAsync(
        string blobName, 
        string contentType, 
        CancellationToken cancellationToken = default)
    {
        // Retorna uma URL fake para testes
        var fakeUrl = $"https://mock-storage.local/documents/{blobName}?sas=mock-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        return Task.FromResult((fakeUrl, expiresAt));
    }

    public Task<(string DownloadUrl, DateTime ExpiresAt)> GenerateDownloadUrlAsync(
        string blobName, 
        CancellationToken cancellationToken = default)
    {
        // Retorna uma URL fake para testes
        var fakeUrl = $"https://mock-storage.local/documents/{blobName}?sas=mock-token";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        return Task.FromResult((fakeUrl, expiresAt));
    }

    public Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        // Sempre retorna true para simplificar testes
        return Task.FromResult(true);
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        // Não faz nada em ambiente de teste
        return Task.CompletedTask;
    }
}
