using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handler responsável por rejeitar documentos após verificação manual.
/// </summary>
public class RejectDocumentCommandHandler(
    IDocumentRepository repository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RejectDocumentCommandHandler> logger)
    : ICommandHandler<RejectDocumentCommand, Result>
{
    private readonly IDocumentRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<RejectDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(RejectDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Rejecting document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

            // Validar se o documento existe
            var document = await _repository.GetByIdAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for rejection", command.DocumentId);
                throw new NotFoundException("Document", command.DocumentId.ToString());
            }

            // Verificar autorização - apenas admins podem rejeitar documentos
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("HTTP context not available");

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var isAdmin = user.IsInRole("admin") || user.IsInRole("system-admin");
            if (!isAdmin)
            {
                var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
                _logger.LogWarning(
                    "User {UserId} attempted to reject document {DocumentId} without admin privileges",
                    userId, command.DocumentId);
                throw new ForbiddenAccessException("Only administrators can reject documents", null!);
            }

            // Verificar se o documento está em estado válido para rejeição
            if (document.Status != EDocumentStatus.PendingVerification)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be rejected in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    $"Document is in {document.Status} status and can only be rejected when in PendingVerification status"));
            }

            // Validar motivo de rejeição
            if (string.IsNullOrWhiteSpace(command.RejectionReason))
            {
                return Result.Failure(Error.BadRequest("Rejection reason is required"));
            }

            // Rejeitar o documento
            document.MarkAsRejected(command.RejectionReason);
            
            await _repository.UpdateAsync(document, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} rejected successfully. Reason: {Reason}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.RejectionReason, command.CorrelationId);

            return Result.Success();
        }
        catch (NotFoundException)
        {
            throw; // Re-throw para GlobalExceptionHandler tratar
        }
        catch (UnauthorizedAccessException)
        {
            throw; // Re-throw para GlobalExceptionHandler tratar
        }
        catch (ForbiddenAccessException)
        {
            throw; // Re-throw para GlobalExceptionHandler tratar
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error rejecting document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);
            return Result.Failure(Error.Internal("Failed to reject document. Please try again later."));
        }
    }
}
