using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Application.Interfaces;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class UploadDocumentCommandHandler : ICommandHandler<UploadDocumentCommand, UploadDocumentResponse>
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

    public async Task<UploadDocumentResponse> HandleAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Gerando URL de upload para documento do provedor {ProviderId}", command.ProviderId);

        if (!Enum.TryParse<DocumentType>(command.DocumentType, true, out var documentType))
        {
            throw new ArgumentException($"Tipo de documento inválido: {command.DocumentType}");
        }

        // Validações básicas
        if (command.FileSizeBytes > 10 * 1024 * 1024) // 10MB
        {
            throw new ArgumentException("Arquivo muito grande. Máximo: 10MB");
        }

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
        if (!allowedContentTypes.Contains(command.ContentType.ToLowerInvariant()))
        {
            throw new ArgumentException($"Tipo de arquivo não permitido: {command.ContentType}");
        }

        // Gera nome único do blob
        var extension = Path.GetExtension(command.FileName);
        var blobName = $"documents/{command.ProviderId}/{Guid.NewGuid()}{extension}";

        // Gera SAS token para upload direto
        var (uploadUrl, expiresAt) = await _blobStorageService.GenerateUploadUrlAsync(
            blobName,
            command.ContentType,
            cancellationToken);

        // Cria registro do documento (status: Uploaded)
        var document = Document.Create(
            command.ProviderId,
            documentType,
            command.FileName,
            blobName); // Armazena o nome do blob, não a URL completa com SAS

        await _documentRepository.AddAsync(document, cancellationToken);
        await _documentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Documento {DocumentId} criado para provedor {ProviderId}", 
            document.Id, command.ProviderId);

        return new UploadDocumentResponse(
            document.Id,
            uploadUrl,
            blobName,
            expiresAt);
    }
}
