using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Helpers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handler responsável por rejeitar documentos após verificação manual.
/// </summary>
public class RejectDocumentCommandHandler(
    IUnitOfWork uow,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RejectDocumentCommandHandler> logger)
    : ICommandHandler<RejectDocumentCommand, Result>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<RejectDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(RejectDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Rejecting document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

            // Verificar autorização - apenas admins podem rejeitar documentos
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("HTTP context not available");

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
            if (!isAdmin)
                throw new ForbiddenAccessException("Apenas administradores podem rejeitar documentos");

            // Validar se o documento existe
            var repository = uow.GetRepository<Document, DocumentId>();
            var document = await repository.TryFindAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for rejection", command.DocumentId);
                throw new NotFoundException("Document", command.DocumentId.ToString());
            }

            // Verificar se o documento está em estado válido para rejeição
            if (document.Status != EDocumentStatus.PendingVerification)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be rejected in status {Status}",
                    command.DocumentId, document.Status);
                
                var statusDescricao = document.Status.ToPortuguese();
                
                return Result.Failure(Error.BadRequest(
                    $"O documento está com status {statusDescricao} e só pode ser recusado quando estiver em Verificação Pendente"));
            }

            // Validar motivo de rejeição
            if (string.IsNullOrWhiteSpace(command.RejectionReason))
            {
                return Result.Failure(Error.BadRequest("Motivo de recusa é obrigatório"));
            }

            // Rejeitar o documento
            document.MarkAsRejected(command.RejectionReason);
            
            await uow.SaveChangesAsync(cancellationToken);

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
            return Result.Failure(Error.Internal("Falha ao rejeitar o documento. Por favor, tente novamente mais tarde."));
        }
    }
}
