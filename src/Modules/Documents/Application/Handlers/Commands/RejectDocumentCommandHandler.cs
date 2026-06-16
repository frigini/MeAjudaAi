using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Helpers;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers.Commands;

public class RejectDocumentCommandHandler(
    [FromKeyedServices(ModuleKeys.Documents)] IUnitOfWork uow,
    IDocumentQueries documentQueries,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RejectDocumentCommandHandler> logger)
    : ICommandHandler<RejectDocumentCommand, Result>
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<RejectDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(RejectDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Rejecting document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

            var document = await _documentQueries.GetByIdAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for rejection", command.DocumentId);
                throw new NotFoundException("Document", command.DocumentId.ToString());
            }

            var httpContext = _httpContextAccessor.HttpContext ?? throw new UnauthorizedAccessException("HTTP context not available");
            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
            if (!isAdmin)
            {
                var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
                _logger.LogWarning(
                    "User {UserId} attempted to reject document {DocumentId} without admin privileges",
                    userId, command.DocumentId);
                throw new ForbiddenAccessException("Only administrators can reject documents");
            }

            if (document.Status != EDocumentStatus.PendingVerification)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be rejected in status {Status}",
                    command.DocumentId, document.Status);
                
                var statusDescricao = document.Status.ToPortuguese();
                
                return Result.Failure(Error.BadRequest(
                    $"O documento está com status {statusDescricao} e só pode ser recusado quando estiver em Verificação Pendente", "BadRequest"));
            }

            if (string.IsNullOrWhiteSpace(command.RejectionReason))
            {
                return Result.Failure(Error.BadRequest("Motivo de recusa é obrigatório", "BadRequest"));
            }

            document.MarkAsRejected(command.RejectionReason);
            
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} rejected successfully. Reason: {Reason}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.RejectionReason, command.CorrelationId);

            return Result.Success();
        }
        catch (NotFoundException) { throw; }
        catch (UnauthorizedAccessException) { throw; }
        catch (ForbiddenAccessException) { throw; }
        catch (Exception ex)
        {
            if (IsCriticalException(ex))
                throw;

            _logger.LogError(ex,
                "Unexpected error rejecting document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);
            return Result.Failure(Error.Internal("Falha ao rejeitar o documento. Por favor, tente novamente mais tarde.", "InternalError"));
        }
    }

    private static bool IsCriticalException(Exception ex)
    {
        // Rethrow fatal and cancellation-related exceptions so they are not swallowed.
        return ex is OutOfMemoryException
            or StackOverflowException
            or AccessViolationException
            or System.Threading.ThreadAbortException
            or System.Runtime.InteropServices.SEHException;
    }
}
