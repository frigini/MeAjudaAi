using System.Text.Json;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Shared;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Manipula solicitações para iniciar a verificação de documentos.
/// </summary>
public class RequestVerificationCommandHandler(
    IDocumentsUnitOfWork uow,
    IDocumentQueries documentQueries,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RequestVerificationCommandHandler> logger)
    : ICommandHandler<RequestVerificationCommand, Result>
{
    private readonly IDocumentsUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<RequestVerificationCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(RequestVerificationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _documentQueries.GetByIdAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for verification request", command.DocumentId);
                return Result.Failure(Error.NotFound($"Documento com ID {command.DocumentId} não encontrado", "NotFound"));
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Result.Failure(Error.Unauthorized("Contexto HTTP não disponível", "Unauthorized"));

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return Result.Failure(Error.Unauthorized("Usuário não autenticado", "Unauthorized"));

            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(Error.Unauthorized("ID do usuário não encontrado no token", "Unauthorized"));

            if (!Guid.TryParse(userId, out var userGuid) || userGuid != document.ProviderId)
            {
                var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
                if (!isAdmin)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to request verification for document {DocumentId} owned by provider {ProviderId}",
                        userId, command.DocumentId, document.ProviderId);
                    return Result.Failure(Error.Unauthorized(
                        "Você não tem autorização para solicitar a verificação deste documento", "Unauthorized"));
                }
            }

            if (document.Status != EDocumentStatus.Uploaded &&
                document.Status != EDocumentStatus.Failed)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be marked for verification in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    $"Documento está com status {document.Status} e não pode ser marcado para verificação", "BadRequest"));
            }

            document.MarkAsPendingVerification();

            var outboxRepository = _uow.GetRepository<OutboxMessage, Guid>();
            var payload = JsonSerializer.Serialize(new { documentId = command.DocumentId }, SerializationDefaults.Default);

            var outboxMessage = OutboxMessage.Create(
                type: OutboxMessageTypes.DocumentVerification,
                payload: payload,
                priority: ECommunicationPriority.Normal
            );

            outboxRepository.Add(outboxMessage);

            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document {DocumentId} marked for verification and outbox message created", command.DocumentId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting verification for document {DocumentId}", command.DocumentId);
            return Result.Failure(Error.Internal("Falha ao solicitar a verificação. Por favor, tente novamente mais tarde.", "InternalError"));
        }
    }
}
