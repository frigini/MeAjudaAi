using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class UploadDocumentCommandHandler(
    IDocumentRepository documentRepository,
    IBlobStorageService blobStorageService,
    IBackgroundJobService backgroundJobService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UploadDocumentCommandHandler> logger) : ICommandHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IDocumentRepository _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    private readonly IBackgroundJobService _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<UploadDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UploadDocumentResponse> HandleAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        // Resource-level authorization: user must match the ProviderId or have admin permissions
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new UnauthorizedAccessException("HTTP context not available");

        var user = httpContext.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            throw new UnauthorizedAccessException("User is not authenticated");

        var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        // Check if user matches the provider ID (convert userId to Guid)
        if (!Guid.TryParse(userId, out var userGuid) || userGuid != command.ProviderId)
        {
            // Check if user has admin role
            var isAdmin = user.IsInRole("admin") || user.IsInRole("system-admin");
            if (!isAdmin)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to upload document for provider {ProviderId} without authorization",
                    userId, command.ProviderId);
                throw new UnauthorizedAccessException(
                    "You are not authorized to upload documents for this provider");
            }
        }

        _logger.LogInformation("Gerando URL de upload para documento do provedor {ProviderId}", command.ProviderId);

        // Validação de tipo de documento com enum definido
        if (!Enum.TryParse<EDocumentType>(command.DocumentType, true, out var documentType) ||
            !Enum.IsDefined(typeof(EDocumentType), documentType))
        {
            throw new ArgumentException($"Tipo de documento inválido: {command.DocumentType}");
        }

        // Validação de tamanho de arquivo
        if (command.FileSizeBytes > 10 * 1024 * 1024) // 10MB
        {
            throw new ArgumentException("Arquivo muito grande. Máximo: 10MB");
        }

        // Validação null-safe e tolerante a parâmetros de content-type
        if (string.IsNullOrWhiteSpace(command.ContentType))
        {
            throw new ArgumentException("Content-Type é obrigatório");
        }

        var mediaType = command.ContentType.Split(';')[0].Trim().ToLowerInvariant();
        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
        if (!allowedContentTypes.Contains(mediaType))
        {
            throw new ArgumentException($"Tipo de arquivo não permitido: {mediaType}");
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

        // Enfileira job de verificação do documento
        await _backgroundJobService.EnqueueAsync<IDocumentVerificationService>(
            service => service.ProcessDocumentAsync(document.Id, CancellationToken.None));

        return new UploadDocumentResponse(
            document.Id,
            uploadUrl,
            blobName,
            expiresAt);
    }
}
