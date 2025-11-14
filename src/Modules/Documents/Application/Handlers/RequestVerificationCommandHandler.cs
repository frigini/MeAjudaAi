using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Application.Handlers;

public class RequestVerificationCommandHandler(
    IDocumentRepository repository,
    ILogger<RequestVerificationCommandHandler> logger)
    : ICommandHandler<RequestVerificationCommand, Result>
{
    public async Task<Result> HandleAsync(RequestVerificationCommand command, CancellationToken cancellationToken = default)
    {
        // Validar se o documento existe
        var document = await repository.GetByIdAsync(command.DocumentId, cancellationToken);
        if (document == null)
        {
            logger.LogWarning("Document {DocumentId} not found for verification request", command.DocumentId);
            return Result.Failure(Error.NotFound($"Document with ID {command.DocumentId} not found"));
        }

        // Atualizar status do documento para PendingVerification
        document.MarkAsPendingVerification();
        await repository.UpdateAsync(document, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        // Enfileirar job de verificação manual (nota: idealmente criar um job dedicado DocumentVerificationJob)
        // Por enquanto, apenas marcar como pending - o job de verificação será implementado posteriormente
        logger.LogInformation("Document {DocumentId} marked for manual verification", command.DocumentId);

        return Result.Success();
    }
}
