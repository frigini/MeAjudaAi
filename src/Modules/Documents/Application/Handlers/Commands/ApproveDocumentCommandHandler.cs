using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Helpers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers.Commands;

public class ApproveDocumentCommandHandler(
    [FromKeyedServices(ModuleKeys.Documents)] IUnitOfWork uow,
    IDocumentQueries documentQueries,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ApproveDocumentCommandHandler> logger,
    IStringLocalizer<Strings> localizer)
    : ICommandHandler<ApproveDocumentCommand, Result>
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<ApproveDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IStringLocalizer<Strings> _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));

    public async Task<Result> HandleAsync(ApproveDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Approving document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

            var document = await _documentQueries.GetByIdAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for approval", command.DocumentId);
                throw new NotFoundException("Document", command.DocumentId.ToString());
            }

            var httpContext = _httpContextAccessor.HttpContext ?? throw new UnauthorizedAccessException(_localizer["HttpContextNotAvailable"]);
            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException(_localizer["DocumentApproveNotAllowed"]);

            var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
            if (!isAdmin)
            {
                var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
                _logger.LogWarning(
                    "User {UserId} attempted to approve document {DocumentId} without admin privileges",
                    userId, command.DocumentId);
                throw new ForbiddenAccessException(_localizer["AdminOnlyCanApproveDocuments"]);
            }

            if (document.Status != EDocumentStatus.PendingVerification)
            {
                _logger.LogWarning(
                    "Document {DocumentId} cannot be approved in status {Status}",
                    command.DocumentId, document.Status);
                return Result.Failure(Error.BadRequest(
                    _localizer["DocumentStatusInvalidForApproval", document.Status.ToPortuguese()], "BadRequest"));
            }

            var ocrData = command.VerificationNotes != null
                ? System.Text.Json.JsonSerializer.Serialize(new { notes = command.VerificationNotes }, SerializationDefaults.Default)
                : null;

            document.MarkAsVerified(ocrData);

            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} approved successfully. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

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
                "Unexpected error approving document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);
            return Result.Failure(Error.Internal(_localizer["DocumentApproveError"], "InternalError"));
        }

    }

    private static bool IsCriticalException(Exception ex)
    {
        // Rethrow fatal and cancellation-related exceptions so they are not swallowed.
        return ex is OutOfMemoryException
            or StackOverflowException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or System.Runtime.InteropServices.SEHException
            or OperationCanceledException;
    }
}