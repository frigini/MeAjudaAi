using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class GetProviderDocumentsQueryHandler : IQueryHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetProviderDocumentsQueryHandler> _logger;

    public GetProviderDocumentsQueryHandler(
        IDocumentRepository documentRepository,
        ILogger<GetProviderDocumentsQueryHandler> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentDto>> HandleAsync(GetProviderDocumentsQuery query, CancellationToken cancellationToken)
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
