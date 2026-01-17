using System.Text.Json;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Helpers;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Handler responsável por aprovar documentos após verificação manual.
/// </summary>
public class ApproveDocumentCommandHandler(
    IDocumentRepository repository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ApproveDocumentCommandHandler> logger)
    : ICommandHandler<ApproveDocumentCommand, Result>
{
    private readonly IDocumentRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<ApproveDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(ApproveDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Approving document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

            // Validar se o documento existe
            var document = await _repository.GetByIdAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for approval", command.DocumentId);
                throw new NotFoundException("Document", command.DocumentId.ToString());
            }

            // Verificar autorização - apenas admins podem aprovar documentos
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("Contexto HTTP não disponível");

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("É necessário estar autenticado para aprovar documentos");

            var isAdmin = user.IsInRole("admin") || user.IsInRole("system-admin");
            if (!isAdmin)
            {
                var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
                _logger.LogWarning(
                    "User {UserId} attempted to approve document {DocumentId} without admin privileges",
                    userId, command.DocumentId);
                throw new ForbiddenAccessException("Apenas administradores podem aprovar documentos");
            }

            // Verificar se o documento está em estado válido para aprovação
            if (document.Status != EDocumentStatus.PendingVerification)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be approved in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    $"O documento está com o status {document.Status.ToPortuguese()} e só pode ser aprovado quando estiver em Verificação Pendente"));
            }

            // Aprovar o documento
            var ocrData = command.VerificationNotes != null 
                ? JsonSerializer.Serialize(new { notes = command.VerificationNotes })
                : null;
            
            document.MarkAsVerified(ocrData);
            
            await _repository.UpdateAsync(document, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

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
