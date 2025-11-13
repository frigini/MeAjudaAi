using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class GetProviderDocumentsQueryHandler : IRequestHandler<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>
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

    public async Task<IEnumerable<DocumentDto>> Handle(GetProviderDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetByProviderIdAsync(request.ProviderId, cancellationToken);

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
