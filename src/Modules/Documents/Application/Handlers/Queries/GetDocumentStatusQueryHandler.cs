using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class GetDocumentStatusQueryHandler(
    IDocumentQueries documentQueries,
    ILogger<GetDocumentStatusQueryHandler> logger) : IQueryHandler<GetDocumentStatusQuery, DocumentDto?>
{
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));
    private readonly ILogger<GetDocumentStatusQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<DocumentDto?> HandleAsync(GetDocumentStatusQuery query, CancellationToken cancellationToken = default)
    {
        var document = await _documentQueries.GetByIdAsync(query.DocumentId, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Document {DocumentId} not found", query.DocumentId);
            return null;
        }

        return new DocumentDto(
            document.Id,
            document.ProviderId,
            document.DocumentType,
            document.FileName,
            document.FileUrl,
            document.Status,
            document.UploadedAt,
            document.VerifiedAt,
            document.RejectionReason,
            document.OcrData);
    }
}
