using MeAjudaAi.Modules.Documents.Application.Interfaces;

namespace MeAjudaAi.Modules.Documents.Tests.Mocks;

/// <summary>
/// Mock implementation of IBlobStorageService for testing environments
/// </summary>
/// <remarks>
/// Por padrão, sempre retorna sucesso nas operações (upload/download/delete).
/// ExistsAsync retorna false por padrão (blobs não existem até serem registrados).
/// 
/// Para testar cenários positivos (blob existe):
/// - Chame GenerateUploadUrlAsync() para registrar o blob automaticamente
/// - Ou use SetBlobExists(blobName, true) para registrar manualmente
/// 
/// Para testar cenários negativos (blob não existe):
/// - Não registre o blob (comportamento padrão)
/// - Ou use SetBlobExists(blobName, false) para remover blob previamente registrado
/// 
/// Acompanhe blobs deletados via DeletedBlobs para verificar comportamento de limpeza.
/// </remarks>
public sealed class MockBlobStorageService : IBlobStorageService
{
    private readonly HashSet<string> _existingBlobs = [];
    private readonly List<string> _deletedBlobs = [];

    /// <summary>
    /// Blobs que foram deletados durante os testes (útil para asserções)
    /// </summary>
    public IReadOnlyList<string> DeletedBlobs => _deletedBlobs.AsReadOnly();

    /// <summary>
    /// Configura se um blob específico existe (útil para testar fluxo "blob não encontrado")
    /// </summary>
    public void SetBlobExists(string blobName, bool exists)
    {
        if (exists)
            _existingBlobs.Add(blobName);
        else
            _existingBlobs.Remove(blobName);
    }

    /// <summary>
    /// Reseta o estado do mock
    /// </summary>
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
        // Ao gerar upload URL, assume que o blob vai existir após upload
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
        // Retorna true apenas se o blob foi explicitamente adicionado à coleção
        return Task.FromResult(_existingBlobs.Contains(blobName));
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        _existingBlobs.Remove(blobName);
        _deletedBlobs.Add(blobName);
        return Task.CompletedTask;
    }
}
