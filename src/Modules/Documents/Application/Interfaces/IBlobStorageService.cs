namespace MeAjudaAi.Modules.Documents.Application.Interfaces;

/// <summary>
/// Serviço para gerenciamento de arquivos no Azure Blob Storage
/// </summary>
public interface IBlobStorageService
{
    /// <summary>
    /// Gera uma URL com SAS token para upload direto ao blob storage
    /// </summary>
    /// <param name="blobName">Nome/caminho do blob</param>
    /// <param name="contentType">Content-Type do arquivo</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Tupla com (URL de upload, data de expiração)</returns>
    Task<(string UploadUrl, DateTime ExpiresAt)> GenerateUploadUrlAsync(
        string blobName,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gera uma URL com SAS token para download/leitura de um blob
    /// </summary>
    /// <param name="blobName">Nome/caminho do blob</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Tupla com (URL de download, data de expiração)</returns>
    Task<(string DownloadUrl, DateTime ExpiresAt)> GenerateDownloadUrlAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se um blob existe
    /// </summary>
    Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deleta um blob
    /// </summary>
    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
}
