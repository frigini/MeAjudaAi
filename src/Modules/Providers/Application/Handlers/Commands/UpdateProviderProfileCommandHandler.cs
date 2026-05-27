using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de atualização de perfil do prestador de serviços.
/// </summary>
public sealed class UpdateProviderProfileCommandHandler(
    IUnitOfWork uow,
    ILogger<UpdateProviderProfileCommandHandler> logger
) : ICommandHandler<UpdateProviderProfileCommand, Result<ProviderDto>>
{
    public async Task<Result<ProviderDto>> HandleAsync(UpdateProviderProfileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating provider profile {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result<ProviderDto>.Failure(Error.NotFound(ValidationMessages.Providers.ProviderNotFound));
            }

            var businessProfile = command.BusinessProfile.ToDomain();
            provider.UpdateProfile(command.Name, businessProfile, command.UpdatedBy);

            if (command.Services != null)
            {
                provider.UpdateServices(command.Services.Select(s => (s.ServiceId, s.ServiceName)));
            }

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider profile {ProviderId} updated successfully", command.ProviderId);
            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating provider profile {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure("Error updating provider profile. Please try again later.");
        }
    }
}
