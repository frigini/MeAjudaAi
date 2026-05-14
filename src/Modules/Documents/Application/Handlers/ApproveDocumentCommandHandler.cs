using System.Text.Json;
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

using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handler responsável por aprovar documentos após verificação manual.
/// </summary>
public class ApproveDocumentCommandHandler(
    IUnitOfWork uow,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ApproveDocumentCommandHandler> logger)
    : ICommandHandler<ApproveDocumentCommand, Result>
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<ApproveDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(ApproveDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Approving document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

            // Verificar autorização primeiro (prevenção de ID enumeration)
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("Contexto HTTP não disponível");

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("É necessário estar autenticado para aprovar documentos");

            var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
            if (!isAdmin)
            {
                var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
                _logger.LogWarning(
                    "User {UserId} attempted to approve document {DocumentId} without admin privileges",
                    userId, command.DocumentId);
                throw new ForbiddenAccessException("Apenas administradores podem aprovar documentos");
            }

            // Validar se o documento existe
            var repository = _uow.GetRepository<Document, DocumentId>();
            var document = await repository.TryFindAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for approval", command.DocumentId);
                throw new NotFoundException("Document", command.DocumentId.ToString());
            }

            // Verificar se o documento está em estado válido para aprovação
            if (document.Status != EDocumentStatus.PendingVerification)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be approved in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    $"O documento está com o status {document.Status.ToPortuguese()} e só pode ser aprovado quando estiver em Verificação Pendente",
                    ErrorCodes.BadRequest));
            }

            // Aprovar o documento
            var ocrData = command.VerificationNotes != null 
                ? JsonSerializer.Serialize(new { notes = command.VerificationNotes })
                : null;
            
            document.MarkAsVerified(ocrData);
            
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} approved successfully. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

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
                "Unexpected error approving document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);
            return Result.Failure(Error.Internal("Falha ao aprovar o documento. Por favor, tente novamente mais tarde."));
        }
    }
}
