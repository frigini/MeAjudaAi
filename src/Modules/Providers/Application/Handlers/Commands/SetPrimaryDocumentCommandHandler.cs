using System.Linq;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler para o comando de definir documento como primário
/// </summary>
public sealed class SetPrimaryDocumentCommandHandler(
    IProviderRepository providerRepository,
    ILogger<SetPrimaryDocumentCommandHandler> logger) : ICommandHandler<SetPrimaryDocumentCommand, Result<ProviderDto>>
{
    public async Task<Result<ProviderDto>> HandleAsync(SetPrimaryDocumentCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Setting primary document {DocumentType} for provider {ProviderId}",
            command.DocumentType, command.ProviderId);

        try
        {
            var provider = await providerRepository.GetByIdAsync(command.ProviderId, cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result<ProviderDto>.Failure(Error.NotFound($"Provider with ID {command.ProviderId} not found"));
            }

            // Verifica se o documento existe antes de definir como primário
            if (!provider.Documents.Any(d => d.DocumentType == command.DocumentType))
            {
                logger.LogWarning("Document {DocumentType} not found for provider {ProviderId}", command.DocumentType, command.ProviderId);
                return Result<ProviderDto>.Failure(Error.NotFound($"Document of type {command.DocumentType} not found for provider {command.ProviderId}"));
            }

            // Define o documento como primário
            provider.SetPrimaryDocument(command.DocumentType);

            // Salva as alterações
            await providerRepository.UpdateAsync(provider, cancellationToken);

            var providerDto = provider.ToDto();
            logger.LogInformation("Primary document set successfully for provider {ProviderId}", command.ProviderId);

            return Result<ProviderDto>.Success(providerDto);
        }
        catch (ProviderDomainException ex)
        {
            logger.LogWarning(ex, "Business rule violation while setting primary document for provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure(Error.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting primary document for provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure(Error.Internal($"Error setting primary document: {ex.Message}"));
        }
    }
}
