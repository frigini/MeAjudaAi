using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Options;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MeAjudaAi.Modules.Documents.Domain;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Manipula comandos de upload de documentos gerando URLs SAS e persistindo metadados do documento.
/// </summary>
public class UploadDocumentCommandHandler(
    IUnitOfWork uow,
    IBlobStorageService blobStorageService,
    IBackgroundJobService backgroundJobService,
    IHttpContextAccessor httpContextAccessor,
    IOptions<DocumentUploadOptions> uploadOptions,
    MeAjudaAi.Shared.Database.Outbox.IOutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> outboxRepository,
    ILogger<UploadDocumentCommandHandler> logger) : ICommandHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    private readonly IBackgroundJobService _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly DocumentUploadOptions _uploadOptions = uploadOptions?.Value ?? throw new ArgumentNullException(nameof(uploadOptions));
    private readonly MeAjudaAi.Shared.Database.Outbox.IOutboxRepository<MeAjudaAi.Shared.Database.Outbox.OutboxMessage> _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
    private readonly ILogger<UploadDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UploadDocumentResponse> HandleAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Autorização no nível do recurso: o usuário deve corresponder ao ProviderId ou possuir permissões de administrador
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("HTTP context not available");

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token");

            // Verificar se o usuário corresponde ao ID do provedor (converter userId para Guid)
            if (!Guid.TryParse(userId, out var userGuid) || userGuid != command.ProviderId)
            {
                // Verificar se o usuário possui o papel de administrador
                var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
                if (!isAdmin)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to upload document for provider {ProviderId} without authorization",
                        userId, command.ProviderId);
                    throw new UnauthorizedAccessException(
                        "You are not authorized to upload documents for this provider");
                }
            }

            _logger.LogInformation("Generating upload URL for provider {ProviderId} document", command.ProviderId);

            // Validação de tipo de documento com enum definido
            if (!Enum.TryParse<EDocumentType>(command.DocumentType, true, out var documentType) ||
                !Enum.IsDefined(typeof(EDocumentType), documentType))
            {
                throw new ArgumentException($"Invalid document type: {command.DocumentType}");
            }

            // Validação de tamanho de arquivo (específico por tipo ou global)
            var maxFileSize = _uploadOptions.GetMaxFileSizeForDocumentType(command.DocumentType);
            if (command.FileSizeBytes > maxFileSize)
            {
                var maxSizeMB = maxFileSize / (1024.0 * 1024.0);
                throw new ArgumentException(
                    $"File too large for {command.DocumentType}. Maximum: {maxSizeMB:F1}MB");
            }

            // Validação null-safe e tolerante a parâmetros de content-type
            if (string.IsNullOrWhiteSpace(command.ContentType))
            {
                throw new ArgumentException("Content-Type is required");
            }

            var mediaType = command.ContentType.Split(';')[0].Trim().ToLowerInvariant();
            if (!_uploadOptions.AllowedContentTypes.Contains(mediaType))
            {
                throw new ArgumentException($"File type not allowed: {mediaType}. Allowed types: {string.Join(", ", _uploadOptions.AllowedContentTypes)}");
            }

            // Gera nome único do blob
            var extension = Path.GetExtension(command.FileName);
            var blobName = $"documents/{command.ProviderId}/{Guid.NewGuid()}{extension}";

            _logger.LogInformation("Generating upload URL for blob {BlobName}", blobName);
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

            _uow.GetRepository<Document, DocumentId>().Add(document);

            // Cria mensagem de outbox para o job de verificação
            var outboxMessage = MeAjudaAi.Shared.Database.Outbox.OutboxMessage.Create(
                type: MeAjudaAi.Shared.Database.Outbox.OutboxMessageTypes.DocumentVerification,
                payload: System.Text.Json.JsonSerializer.Serialize(new { DocumentId = document.Id.Value }),
                priority: MeAjudaAi.Contracts.Shared.ECommunicationPriority.Normal);

            _logger.LogInformation("Saving document and outbox message to database for provider {ProviderId}", command.ProviderId);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document {DocumentId} created and job enqueued in outbox for provider {ProviderId}",
                document.Id, command.ProviderId);

            return new UploadDocumentResponse(
                document.Id,
                uploadUrl,
                blobName,
                expiresAt);
        }

        catch (Exception ex) when (ex is not UnauthorizedAccessException and not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error while uploading document for provider {ProviderId}", command.ProviderId);
            throw new InvalidOperationException(ValidationMessages.UploadFailed, ex);
        }
    }
}
