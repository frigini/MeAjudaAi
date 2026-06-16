using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class GetProviderDocumentsQueryHandler(
    IDocumentQueries documentQueries) : IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>
{
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));

    public async Task<IEnumerable<DocumentDto>> HandleAsync(GetProviderDocumentsQuery query, CancellationToken cancellationToken = default)
    {
        var documents = await _documentQueries.GetByProviderIdAsync(query.ProviderId, cancellationToken);

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
