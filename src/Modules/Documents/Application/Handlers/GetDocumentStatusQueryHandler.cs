using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handles queries to retrieve the current status of a specific document.
/// </summary>
public class GetDocumentStatusQueryHandler(
    IDocumentQueries queries,
    ILogger<GetDocumentStatusQueryHandler> logger) : IQueryHandler<GetDocumentStatusQuery, DocumentDto?>
{
    private readonly IDocumentQueries _queries = queries ?? throw new ArgumentNullException(nameof(queries));
    private readonly ILogger<GetDocumentStatusQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<DocumentDto?> HandleAsync(GetDocumentStatusQuery query, CancellationToken cancellationToken = default)
    {
        var document = await _queries.GetByIdAsync(query.DocumentId, cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Documento {DocumentId} não encontrado", query.DocumentId);
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
