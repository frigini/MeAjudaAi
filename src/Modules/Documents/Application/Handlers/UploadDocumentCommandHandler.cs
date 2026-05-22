using System.Text.Json;
using MeAjudaAi.Contracts.Shared;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Options;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class UploadDocumentCommandHandler(
    IDocumentsUnitOfWork uow,
    IDocumentQueries documentQueries,
    IBlobStorageService blobStorageService,
    IHttpContextAccessor httpContextAccessor,
    IOptions<DocumentUploadOptions> uploadOptions,
    ILogger<UploadDocumentCommandHandler> logger) : ICommandHandler<UploadDocumentCommand, UploadDocumentResponse>
{
    private readonly IDocumentsUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));
    private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly DocumentUploadOptions _uploadOptions = uploadOptions?.Value ?? throw new ArgumentNullException(nameof(uploadOptions));
    private readonly ILogger<UploadDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<UploadDocumentResponse> HandleAsync(UploadDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("HTTP context not available");

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User ID not found in token");

            if (!Guid.TryParse(userId, out var userGuid) || userGuid != command.ProviderId)
            {
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

            if (!Enum.TryParse<EDocumentType>(command.DocumentType, true, out var documentType) ||
                !Enum.IsDefined(typeof(EDocumentType), documentType))
            {
                throw new ArgumentException($"Invalid document type: {command.DocumentType}");
            }

            var maxFileSize = _uploadOptions.GetMaxFileSizeForDocumentType(command.DocumentType);
            if (command.FileSizeBytes > maxFileSize)
            {
                var maxSizeMB = maxFileSize / (1024.0 * 1024.0);
                throw new ArgumentException(
                    $"File too large for {command.DocumentType}. Maximum: {maxSizeMB:F1}MB");
            }

            if (string.IsNullOrWhiteSpace(command.ContentType))
            {
                throw new ArgumentException("Content-Type is required");
            }

            var mediaType = command.ContentType.Split(';')[0].Trim().ToLowerInvariant();
            if (!_uploadOptions.AllowedContentTypes.Contains(mediaType))
            {
                throw new ArgumentException($"File type not allowed: {mediaType}. Allowed types: {string.Join(", ", _uploadOptions.AllowedContentTypes)}");
            }

            var extension = Path.GetExtension(command.FileName);
            var blobName = $"documents/{command.ProviderId}/{Guid.NewGuid()}{extension}";

            var (uploadUrl, expiresAt) = await _blobStorageService.GenerateUploadUrlAsync(
                blobName,
                command.ContentType,
                cancellationToken);

            var document = Document.Create(
                command.ProviderId,
                documentType,
                command.FileName,
                blobName);

            _uow.GetRepository<Document, Guid>().Add(document);

            var outboxRepository = _uow.GetRepository<OutboxMessage, Guid>();
            var payload = JsonSerializer.Serialize(new { documentId = document.Id }, SerializationDefaults.Default);

            var outboxMessage = OutboxMessage.Create(
                type: OutboxMessageTypes.DocumentVerification,
                payload: payload,
                priority: ECommunicationPriority.Normal
            );

            outboxRepository.Add(outboxMessage);

            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document {DocumentId} created for provider {ProviderId} and outbox message created",
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
