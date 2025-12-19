using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handles queries to retrieve the current status of a specific document.
/// </summary>
/// <param name="documentRepository">Document repository for data access.</param>
/// <param name="logger">Logger instance.</param>
public class GetDocumentStatusQueryHandler(
    IDocumentRepository documentRepository,
    ILogger<GetDocumentStatusQueryHandler> logger) : IQueryHandler<GetDocumentStatusQuery, DocumentDto?>
{
    private readonly IDocumentRepository _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    private readonly ILogger<GetDocumentStatusQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<DocumentDto?> HandleAsync(GetDocumentStatusQuery query, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(query.DocumentId, cancellationToken);

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
