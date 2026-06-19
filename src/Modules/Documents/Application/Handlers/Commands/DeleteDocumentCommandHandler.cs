using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers.Commands;

public class DeleteDocumentCommandHandler(
    [FromKeyedServices(ModuleKeys.Documents)] IUnitOfWork uow,
    IDocumentQueries documentQueries,
    IBlobStorageService blobStorageService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<DeleteDocumentCommandHandler> logger)
    : ICommandHandler<DeleteDocumentCommand, Result>
{
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IDocumentQueries _documentQueries = documentQueries ?? throw new ArgumentNullException(nameof(documentQueries));
    private readonly IBlobStorageService _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<DeleteDocumentCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<Result> HandleAsync(DeleteDocumentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Deleting document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);

            var document = await _documentQueries.GetByIdAsync(command.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found for deletion", command.DocumentId);
                throw new NotFoundException("Document", command.DocumentId.ToString());
            }

            var httpContext = _httpContextAccessor.HttpContext ?? throw new UnauthorizedAccessException("Contexto HTTP não disponível");
            var user = httpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("É necessário estar autenticado para excluir documentos");

            var isAdmin = RoleConstants.AdminEquivalentRoles.Any(user.IsInRole);
            if (!isAdmin)
            {
                var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
                _logger.LogWarning(
                    "User {UserId} attempted to delete document {DocumentId} without admin privileges",
                    userId, command.DocumentId);
                throw new ForbiddenAccessException("Apenas administradores podem excluir documentos");
            }

            if (!string.IsNullOrEmpty(document.FileUrl))
            {
                await _blobStorageService.DeleteAsync(document.FileUrl, cancellationToken);
            }

            var repository = _uow.GetRepository<Modules.Documents.Domain.Entities.Document, Guid>();
            var entity = await repository.TryFindAsync(command.DocumentId, cancellationToken);
            if (entity is not null)
            {
                repository.Delete(entity);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Document {DocumentId} deleted successfully. CorrelationId: {CorrelationId}",
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
                "Unexpected error deleting document {DocumentId}. CorrelationId: {CorrelationId}",
                command.DocumentId, command.CorrelationId);
            return Result.Failure(Error.Internal("Falha ao excluir o documento. Por favor, tente novamente mais tarde.", "InternalError"));
        }
    }

    private static bool IsCriticalException(Exception ex)
    {
        return ex is OutOfMemoryException
            or StackOverflowException
            or AccessViolationException
            or AppDomainUnloadedException
            or BadImageFormatException
            or System.Runtime.InteropServices.SEHException
            or OperationCanceledException;
    }
}
