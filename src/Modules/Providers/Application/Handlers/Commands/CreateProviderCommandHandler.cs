using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de criação de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para criação de prestadores de serviços, incluindo validações de negócio,
/// verificação de duplicidade de usuário e integração com serviços de domínio.
/// </remarks>
/// <param name="providerRepository">Repositório para persistência de prestadores de serviços</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class CreateProviderCommandHandler(
    IProviderRepository providerRepository,
    ILogger<CreateProviderCommandHandler> logger
) : ICommandHandler<CreateProviderCommand, Result<ProviderDto>>
{
    /// <summary>
    /// Processa o comando de criação de prestador de serviços de forma assíncrona.
    /// </summary>
    /// <param name="command">Comando de criação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação com o DTO do prestador de serviços criado</returns>
    public async Task<Result<ProviderDto>> HandleAsync(CreateProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Creating provider for user {UserId}", command.UserId);

            // Verifica se já existe um prestador de serviços para este usuário
            var existingProvider = await providerRepository.GetByUserIdAsync(command.UserId, cancellationToken);
            if (existingProvider != null)
            {
                logger.LogWarning("Provider already exists for user {UserId}", command.UserId);
                return Result<ProviderDto>.Failure("Provider already exists for this user");
            }

            // Converte DTOs para objetos de domínio
            var businessProfile = command.BusinessProfile.ToDomain();

            // Cria a entidade de domínio
            var provider = new Provider(
                command.UserId,
                command.Name,
                command.Type,
                businessProfile
            );

            // Persiste no repositório
            await providerRepository.AddAsync(provider, cancellationToken);

            logger.LogInformation("Provider {ProviderId} created successfully for user {UserId}",
                provider.Id.Value, command.UserId);

            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating provider for user {UserId}", command.UserId);
            return Result<ProviderDto>.Failure("An error occurred while creating the provider");
        }
    }
}
