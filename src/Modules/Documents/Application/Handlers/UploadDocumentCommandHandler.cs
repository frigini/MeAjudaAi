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

/// <summary>
/// Manipula comandos de upload de documentos gerando URLs SAS e persistindo metadados do documento.
/// </summary>
/// <param name="documentRepository">Repositório de documentos para acesso a dados.</param>
/// <param name="blobStorageService">Serviço para operações de armazenamento de blobs.</param>
/// <param name="backgroundJobService">Serviço para enfileirar jobs em segundo plano.</param>
/// <param name="httpContextAccessor">Acessor para o contexto HTTP.</param>
/// <param name="logger">Instância do logger.</param>
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

            _logger.LogInformation("Generating upload URL for provider {ProviderId} document", command.ProviderId);

            // Validação de tipo de documento com enum definido
            if (!Enum.TryParse<EDocumentType>(command.DocumentType, true, out var documentType) ||
                !Enum.IsDefined(typeof(EDocumentType), documentType))
            {
                throw new ArgumentException($"Invalid document type: {command.DocumentType}");
            }

            // Validação de tamanho de arquivo
            if (command.FileSizeBytes > 10 * 1024 * 1024) // 10MB
            {
                throw new ArgumentException("File too large. Maximum: 10MB");
            }

            // Validação null-safe e tolerante a parâmetros de content-type
            if (string.IsNullOrWhiteSpace(command.ContentType))
            {
                throw new ArgumentException("Content-Type is required");
            }

            var mediaType = command.ContentType.Split(';')[0].Trim().ToLowerInvariant();
            // TODO: Considerar tornar o limite de tamanho e os tipos permitidos configuráveis via appsettings.json
            //       quando surgirem requisitos diferentes para ambientes de implantação
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/jpg", "application/pdf" };
            if (!allowedContentTypes.Contains(mediaType))
            {
                throw new ArgumentException($"File type not allowed: {mediaType}");
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

            _logger.LogInformation("Document {DocumentId} created for provider {ProviderId}",
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
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Authorization failed while uploading document for provider {ProviderId}", command.ProviderId);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed while uploading document: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while uploading document for provider {ProviderId}", command.ProviderId);
            throw new InvalidOperationException("Failed to upload document. Please try again later.", ex);
        }
    }
}
