using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<UploadDocumentCommandHandler> _logger;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IBlobStorageService blobStorageService,
        ILogger<UploadDocumentCommandHandler> logger)
    {
        _documentRepository = documentRepository;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<UploadDocumentResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Gerando URL de upload para documento do provedor {ProviderId}", request.ProviderId);

        if (!Enum.TryParse<DocumentType>(request.DocumentType, true, out var documentType))
        {
            throw new ArgumentException($"Tipo de documento inválido: {request.DocumentType}");
        }

        // Validações básicas
        if (request.FileSizeBytes > 10 * 1024 * 1024) // 10MB
        {
            throw new ArgumentException("Arquivo muito grande. Máximo: 10MB");
        }

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
        if (!allowedContentTypes.Contains(request.ContentType.ToLowerInvariant()))
        {
            throw new ArgumentException($"Tipo de arquivo não permitido: {request.ContentType}");
        }

        // Gera nome único do blob
        var extension = Path.GetExtension(request.FileName);
        var blobName = $"documents/{request.ProviderId}/{Guid.NewGuid()}{extension}";

        // Gera SAS token para upload direto
        var (uploadUrl, expiresAt) = await _blobStorageService.GenerateUploadUrlAsync(
            blobName,
            request.ContentType,
            cancellationToken);

        // Cria registro do documento (status: Uploaded)
        var document = Document.Create(
            request.ProviderId,
            documentType,
            request.FileName,
            blobName); // Armazena o nome do blob, não a URL completa com SAS

        await _documentRepository.AddAsync(document, cancellationToken);
        await _documentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Documento {DocumentId} criado para provedor {ProviderId}", 
            document.Id, request.ProviderId);

        return new UploadDocumentResponse(
            document.Id,
            uploadUrl,
            blobName,
            expiresAt);
    }
}
