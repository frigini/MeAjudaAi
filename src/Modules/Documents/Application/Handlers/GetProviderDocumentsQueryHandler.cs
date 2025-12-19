using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handles queries to retrieve all documents for a specific provider.
/// </summary>
/// <param name="documentRepository">Document repository for data access.</param>
public class GetProviderDocumentsQueryHandler(
    IDocumentRepository documentRepository) : IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));

    public async Task<IEnumerable<DocumentDto>> HandleAsync(GetProviderDocumentsQuery query, CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByProviderIdAsync(query.ProviderId, cancellationToken);

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
