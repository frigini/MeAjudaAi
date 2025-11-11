using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de adição de qualificações a prestadores de serviços.
/// </summary>
/// <param name="providerRepository">Repositório para acesso aos dados</param>
/// <param name="logger">Logger estruturado</param>
public sealed class AddQualificationCommandHandler(
    IProviderRepository providerRepository,
    ILogger<AddQualificationCommandHandler> logger
) : ICommandHandler<AddQualificationCommand, Result<ProviderDto>>
{
    /// <summary>
    /// Processa o comando de adição de qualificação.
    /// </summary>
    public async Task<Result<ProviderDto>> HandleAsync(AddQualificationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Adding qualification to provider {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result<ProviderDto>.Failure("Provider not found");
            }

            var qualification = new Qualification(
                command.Name,
                command.Description,
                command.IssuingOrganization,
                command.IssueDate,
                command.ExpirationDate,
                command.DocumentNumber
            );

            provider.AddQualification(qualification);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Qualification added successfully to provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding qualification to provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure("An error occurred while adding the qualification");
        }
    }
}
