using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar o comando de criação de um novo prestador de serviços.
/// </summary>
/// <remarks>
/// Este handler gerencia o fluxo de criação inicial, validando a unicidade por usuário
/// e persistindo os dados básicos do perfil profissional.
/// </remarks>
/// <param name="uow">Unit of Work para persistência de prestadores de serviços</param>
/// <param name="providerQueries">Serviço de consulta de prestadores</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class CreateProviderCommandHandler(
    IUnitOfWork uow,
    IProviderQueries providerQueries,
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
            if (await providerQueries.ExistsByUserIdAsync(command.UserId, cancellationToken))
            {
                logger.LogWarning("Provider already exists for user {UserId}", command.UserId);
                return Result<ProviderDto>.Failure(ValidationMessages.Providers.AlreadyExists);
            }

            // Converte DTOs para objetos de domínio
            var businessProfile = command.BusinessProfile.ToDomain();

            // Cria a entidade de domínio
            var provider = Provider.Create(
                command.UserId,
                command.Name,
                command.Type,
                businessProfile
            );

            // Persiste no repositório
            uow.GetRepository<Provider, ProviderId>().Add(provider);
            try
            {
                await uow.SaveChangesAsync(cancellationToken);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException postgresEx && postgresEx.SqlState == "23505")
            {
                logger.LogWarning("Provider already exists for user {UserId} (DB Constraint)", command.UserId);
                return Result<ProviderDto>.Failure(ValidationMessages.Providers.AlreadyExists);
            }

            logger.LogInformation("Provider {ProviderId} created successfully for user {UserId}",
                provider.Id.Value, command.UserId);

            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (ProviderDomainException ex)
        {
            logger.LogWarning(ex, "Domain validation error creating provider for user {UserId}", command.UserId);
            return Result<ProviderDto>.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument creating provider for user {UserId}", command.UserId);
            return Result<ProviderDto>.Failure(ex.Message);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating provider for user {UserId}", command.UserId);
            return Result<ProviderDto>.Failure(ValidationMessages.Providers.CreationError);
        }
    }
}
