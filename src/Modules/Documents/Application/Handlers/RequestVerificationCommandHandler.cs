using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handles requests to initiate document verification.
/// </summary>
/// <param name="repository">Document repository for data access.</param>
/// <param name="backgroundJobService">Service for enqueuing background jobs.</param>
/// <param name="httpContextAccessor">Accessor for HTTP context.</param>
/// <param name="logger">Logger instance.</param>
public class RequestVerificationCommandHandler(
    IDocumentRepository repository,
    IBackgroundJobService backgroundJobService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RequestVerificationCommandHandler> logger)
    : ICommandHandler<RequestVerificationCommand, Result>
{
    private readonly IDocumentRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IBackgroundJobService _backgroundJobService = backgroundJobService ?? throw new ArgumentNullException(nameof(backgroundJobService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<RequestVerificationCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(RequestVerificationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validar se o documento existe
            var document = await _repository.GetByIdAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Documento {DocumentId} não encontrado para solicitação de verificação", command.DocumentId);
                return Result.Failure(Error.NotFound($"Document with ID {command.DocumentId} not found"));
            }

            // Resource-level authorization: user must match the ProviderId or have admin permissions
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Result.Failure(Error.Unauthorized("HTTP context not available"));

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return Result.Failure(Error.Unauthorized("User is not authenticated"));

            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(Error.Unauthorized("User ID not found in token"));

            // Check if user matches the provider ID
            if (!Guid.TryParse(userId, out var userGuid) || userGuid != document.ProviderId)
            {
                // Check if user has admin role
                var isAdmin = user.IsInRole("admin") || user.IsInRole("system-admin");
                if (!isAdmin)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to request verification for document {DocumentId} owned by provider {ProviderId}",
                        userId, command.DocumentId, document.ProviderId);
                    return Result.Failure(Error.Unauthorized(
                        "You are not authorized to request verification for this document"));
                }
            }

            // Check if the document is in a valid state for verification request
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
            await _repository.UpdateAsync(document, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            // Enfileirar job de verificação
            await _backgroundJobService.EnqueueAsync<IDocumentVerificationService>(
                service => service.ProcessDocumentAsync(command.DocumentId, CancellationToken.None));

            _logger.LogInformation("Documento {DocumentId} marcado para verificação e job enfileirado", command.DocumentId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao solicitar verificação para documento {DocumentId}", command.DocumentId);
            return Result.Failure(Error.Internal("Failed to request verification. Please try again later."));
        }
    }
}
