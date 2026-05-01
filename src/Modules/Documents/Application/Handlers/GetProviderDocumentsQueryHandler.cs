using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handles queries to retrieve all documents for a specific provider.
/// </summary>
public class GetProviderDocumentsQueryHandler(
    IDocumentQueries queries) : IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>
{
    private readonly IDocumentQueries _queries = queries ?? throw new ArgumentNullException(nameof(queries));

    public async Task<IEnumerable<DocumentDto>> HandleAsync(GetProviderDocumentsQuery query, CancellationToken cancellationToken = default)
    {
        var documents = await _queries.GetByProviderIdAsync(query.ProviderId, cancellationToken);

        return documents.Select(d => new DocumentDto(
            d.Id,
            d.ProviderId,
            d.DocumentType,
            d.FileName,
            d.FileUrl,
            d.Status,
            d.UploadedAt,
            d.VerifiedAt,
            d.RejectionReason,
            d.OcrData)).ToList();
    }
}
