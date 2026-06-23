using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de adição de documentos a prestadores de serviços.
/// </summary>
/// <param name="uow">Unit of Work para persistência</param>
/// <param name="logger">Logger estruturado</param>
public sealed class AddDocumentCommandHandler(
    IUnitOfWork uow,
    ILogger<AddDocumentCommandHandler> logger
) : ICommandHandler<AddDocumentCommand, Result<ProviderDto>>
{
    /// <summary>
    /// Processa o comando de adição de documento.
    /// </summary>
    public async Task<Result<ProviderDto>> HandleAsync(AddDocumentCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding document to provider {ProviderId}", command.ProviderId);

        var providerId = new ProviderId(command.ProviderId);
        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
            return Result<ProviderDto>.Failure("Fornecedor não encontrado");
        }

        var document = new Document(
            command.DocumentNumber,
            command.DocumentType,
            command.FileName,
            command.FileUrl
        );
        provider.AddDocument(document);

        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Document added successfully to provider {ProviderId}", command.ProviderId);
        return Result<ProviderDto>.Success(provider.ToDto());
    }
}