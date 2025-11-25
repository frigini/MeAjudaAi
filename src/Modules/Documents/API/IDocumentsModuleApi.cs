using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Documents.API;

/// <summary>
/// Public API for cross-module communication with Documents module.
/// Allows other modules to query document verification status and provider documents.
/// </summary>
public interface IDocumentsModuleApi
{
    /// <summary>
    /// Checks if a provider has all required documents verified.
    /// Used by Providers module to validate provider activation.
    /// </summary>
    /// <param name="providerId">Provider unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if provider has verified documents, false otherwise</returns>
    Task<Result<bool>> HasVerifiedDocumentsAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents for a specific provider.
    /// </summary>
    /// <param name="providerId">Provider unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of documents with verification status</returns>
    Task<Result<List<DocumentDto>>> GetProviderDocumentsAsync(Guid providerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the verification status of a specific document.
    /// </summary>
    /// <param name="documentId">Document unique identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document verification status (Pending, Verified, Rejected)</returns>
    Task<Result<DocumentVerificationStatus>> GetDocumentStatusAsync(Guid documentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Document data transfer object for cross-module communication.
/// </summary>
public record DocumentDto(
    Guid Id,
    Guid ProviderId,
    string DocumentType,
    DocumentVerificationStatus Status,
    DateTime UploadedAt,
    DateTime? VerifiedAt);

/// <summary>
/// Document verification status enum.
/// </summary>
public enum DocumentVerificationStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2
}
