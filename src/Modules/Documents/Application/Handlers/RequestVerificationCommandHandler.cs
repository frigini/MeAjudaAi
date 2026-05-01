using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Manipula solicitações para iniciar a verificação de documentos.
/// </summary>
public class RequestVerificationCommandHandler(
    IUnitOfWork uow,
    IBackgroundJobService backgroundJobService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RequestVerificationCommandHandler> logger)
    : ICommandHandler<RequestVerificationCommand, Result>
{
    private readonly IBackgroundJobService _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<RequestVerificationCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(RequestVerificationCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("DEBUG: HandleAsync called for {DocumentId}", command.DocumentId);
        try
        {
            // Validar se o documento existe
            var repository = uow.GetRepository<Document, DocumentId>();
            var document = await repository.TryFindAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for verification request", command.DocumentId);
                return Result.Failure(Error.NotFound($"Document with ID {command.DocumentId} not found"));
            }
            _logger.LogInformation("Document {DocumentId} found. ProviderId: {ProviderId}, Status: {Status}", command.DocumentId, document.ProviderId, document.Status);

            // Autorização no nível do recurso: o usuário deve corresponder ao ProviderId ou possuir permissões de administrador
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Result.Failure(Error.Unauthorized("HTTP context not available"));

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return Result.Failure(Error.Unauthorized("User is not authenticated"));

            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(Error.Unauthorized("User ID not found in token"));

            // Verificar se o usuário corresponde ao ID do provedor
            if (!Guid.TryParse(userId, out var userGuid) || userGuid != document.ProviderId)
            {
                // Verificar se o usuário possui o papel de administrador
                var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
                if (!isAdmin)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to request verification for document {DocumentId} owned by provider {ProviderId}",
                        userId, command.DocumentId, document.ProviderId);
                    return Result.Failure(Error.Unauthorized(
                        "You are not authorized to request verification for this document"));
                }
            }

            // Verificar se o documento está em um estado válido para solicitação de verificação
            if (document.Status != EDocumentStatus.Uploaded &&
                document.Status != EDocumentStatus.Failed)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be marked for verification in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    $"Document is in {document.Status} status and cannot be marked for verification"));
            }

            // Atualizar status do documento para PendingVerification
            document.MarkAsPendingVerification();
            _logger.LogInformation("Attempting to save changes to DB for document {DocumentId}", command.DocumentId);
            await uow.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Document {DocumentId} status updated to PendingVerification", command.DocumentId);

            // Enfileirar job de verificação
            try
            {
                await _backgroundJobService.EnqueueAsync<IDocumentVerificationService>(
                    service => service.ProcessDocumentAsync(document.Id, CancellationToken.None));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue verification job for document {DocumentId}", command.DocumentId);
                throw; // Rethrow to let the main handler catch it, but now with context
            }
            _logger.LogInformation("Job enqueued for {DocumentId}", command.DocumentId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting verification for document {DocumentId}", command.DocumentId);
            return Result.Failure(Error.Internal("Failed to request verification. Please try again later."));
        }
    }
}
