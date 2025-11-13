using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class GetDocumentStatusQueryHandler : IQueryHandler<GetDocumentStatusQuery, DocumentDto?>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetDocumentStatusQueryHandler> _logger;

    public GetDocumentStatusQueryHandler(
        IDocumentRepository documentRepository,
        ILogger<GetDocumentStatusQueryHandler> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<DocumentDto?> HandleAsync(GetDocumentStatusQuery query, CancellationToken cancellationToken)
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
