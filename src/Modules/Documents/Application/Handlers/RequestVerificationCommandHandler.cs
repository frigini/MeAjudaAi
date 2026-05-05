using System.Text.Json;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

/// <summary>
/// Manipula solicitações para iniciar a verificação de documentos.
/// </summary>
public class RequestVerificationCommandHandler(
    IUnitOfWork uow,
    IDocumentQueries documentQueries,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RequestVerificationCommandHandler> logger)
    : ICommandHandler<RequestVerificationCommand, Result>
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<RequestVerificationCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));

    public async Task<Result> HandleAsync(RequestVerificationCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("HandleAsync called for {DocumentId}", command.DocumentId);
        try
        {
            // Autorização no nível do recurso: identificar quem é o caller
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Result.Failure(Error.Unauthorized("Contexto HTTP não disponível"));

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return Result.Failure(Error.Unauthorized("Usuário não autenticado"));

            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Result.Failure(Error.Unauthorized("ID do usuário não encontrado no token"));

            var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);

            var repository = _uow.GetRepository<Document, DocumentId>();
            Document? document;

            if (isAdmin)
            {
                document = await repository.TryFindAsync(command.DocumentId, cancellationToken);
            }
            else
            {
                var queryResult = await _documentQueries.GetByIdAndProviderAsync(command.DocumentId, userGuid, cancellationToken);
                if (queryResult == null)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to access document {DocumentId} belonging to another provider or not found",
                        userId, command.DocumentId);
                    return Result.Failure(Error.NotFound($"Documento com ID {command.DocumentId} não encontrado"));
                }
                
                // Precisamos carregar o agregado para mutação
                document = await repository.TryFindAsync(command.DocumentId, cancellationToken);
            }

            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found", command.DocumentId);
                return Result.Failure(Error.NotFound($"Documento com ID {command.DocumentId} não encontrado"));
            }

            // Verificar se o documento está em um estado válido para solicitação de verificação
            if (document.Status != EDocumentStatus.Uploaded &&
                document.Status != EDocumentStatus.Failed)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be marked for verification in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    $"O documento está com status {document.Status} e não pode ser marcado para verificação"));
            }

            // Atualizar status do documento para PendingVerification
            document.MarkAsPendingVerification();

            // Criar mensagem outbox para verificação assíncrona
            var outboxMessage = OutboxMessage.Create(
                OutboxMessageTypes.DocumentVerification,
                JsonSerializer.Serialize(new
                {
                    documentId = document.Id.Value,
                    requestedAt = DateTime.UtcNow
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                ECommunicationPriority.High);

            var outboxRepo = _uow.GetRepository<OutboxMessage, Guid>();
            outboxRepo.Add(outboxMessage);

            try
            {
                await _uow.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save verification request for document {DocumentId}", command.DocumentId);
                return Result.Failure(Error.Internal("Falha ao solicitar verificação do documento."));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error requesting verification for document {DocumentId}", command.DocumentId);
            return Result.Failure(Error.Internal("Falha ao solicitar verificação."));
        }
    }
}
