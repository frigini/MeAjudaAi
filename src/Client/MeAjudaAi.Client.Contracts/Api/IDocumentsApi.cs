using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Contracts.Functional;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IDocumentsApi
{
    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Documents.Base}/upload")]
    Task<Result<UploadDocumentResponse>> UploadDocumentAsync(
        [Body] UploadDocumentRequest request,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Documents.Base}/provider/{{providerId}}")]
    Task<Result<IReadOnlyList<ModuleDocumentDto>>> GetDocumentsByProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken = default);

    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Documents.Base}/{{documentId}}")]
    Task<Result<ModuleDocumentDto>> GetDocumentByIdAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    [Delete($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Documents.Base}/{{documentId}}")]
    Task<Result<bool>> DeleteDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Documents.Base}/{{documentId}}/request-verification")]
    Task RequestDocumentVerificationAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Documents.Base}/{{documentId}}/verify")]
    Task<Result> VerifyDocumentAsync(
        Guid documentId,
        [Body] VerifyDocumentRequest request,
        CancellationToken cancellationToken = default);
}