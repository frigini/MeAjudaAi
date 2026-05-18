using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Shared.Database.Constants;
using System.Text.Json;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class RequestVerificationCommandHandler(
    [FromKeyedServices(ModuleKeys.Documents)] IUnitOfWork uow,
    IDocumentQueries documentQueries,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RequestVerificationCommandHandler> logger)
    : ICommandHandler<RequestVerificationCommand, Result>
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
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
                return Result.Failure(Error.NotFound($"Document with ID {command.DocumentId} not found", "NotFound"));
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Result.Failure(Error.Unauthorized("HTTP context not available", "Unauthorized"));

            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                return Result.Failure(Error.Unauthorized("User is not authenticated", "Unauthorized"));

            var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(Error.Unauthorized("User ID not found in token", "Unauthorized"));

            if (!Guid.TryParse(userId, out var userGuid) || userGuid != document.ProviderId)
            {
                var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
                if (!isAdmin)
                {
                    _logger.LogWarning(
                        "User {UserId} attempted to request verification for document {DocumentId} owned by provider {ProviderId}",
                        userId, command.DocumentId, document.ProviderId);
                    return Result.Failure(Error.Unauthorized(
                        "You are not authorized to request verification for this document", "Unauthorized"));
                }
            }

            if (document.Status != EDocumentStatus.Uploaded &&
                document.Status != EDocumentStatus.Failed)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be marked for verification in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    $"Document is in {document.Status} status and cannot be marked for verification", "BadRequest"));
            }

            document.MarkAsPendingVerification();

            var outboxRepository = _uow.GetRepository<OutboxMessage, Guid>();
            var payload = JsonSerializer.Serialize(new { documentId = command.DocumentId }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

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
            return Result.Failure(Error.Internal("Failed to request verification. Please try again later.", "InternalError"));
        }
    }
}
