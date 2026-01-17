using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar consultas de usuário por ID.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para consultas específicas de usuário utilizando
/// o identificador único. Retorna um único usuário convertido para DTO ou
/// uma mensagem de erro caso não seja encontrado.
/// Utiliza cache distribuído para melhorar performance.
/// </remarks>
/// <param name="userRepository">Repositório para consultas de usuários</param>
/// <param name="usersCacheService">Serviço de cache específico para usuários</param>
/// <param name="logger">Logger para auditoria e rastreamento das operações</param>
internal sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository,
    IUsersCacheService usersCacheService,
    ILogger<GetUserByIdQueryHandler> logger
) : IQueryHandler<GetUserByIdQuery, Result<UserDto>>
{
    /// <summary>
    /// Processa a consulta de usuário por ID de forma assíncrona.
    /// </summary>
    /// <param name="query">Consulta contendo o ID do usuário a ser buscado</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: UserDto com os dados do usuário encontrado
    /// - Falha: Error.NotFound se usuário não existe, Error.Internal para outros erros
    /// </returns>
    /// <remarks>
    /// O processo utiliza cache distribuído para melhorar performance:
    /// 1. Busca primeiro no cache
    /// 2. Se não encontrado, busca no repositório e armazena no cache
    /// 3. Converte para DTO se encontrado ou retorna erro
    /// 
    /// Utiliza value object UserId para garantir type safety.
    /// Todas as exceções são capturadas e convertidas em Result.Failure para manter
    /// um contrato de API consistente baseado no padrão Result.
    /// </remarks>
    public async Task<Result<UserDto>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        logger.LogInformation(
            "Starting user lookup by ID with cache. CorrelationId: {CorrelationId}, UserId: {UserId}",
            correlationId, query.UserId);

        try
        {
            // Busca no cache primeiro, depois no repositório se necessário
            var userDto = await usersCacheService.GetOrCacheUserByIdAsync(
                query.UserId,
                async ct =>
                {
                    logger.LogDebug("Cache miss - fetching user from repository. UserId: {UserId}", query.UserId);

                    var user = await userRepository.GetByIdAsync(new UserId(query.UserId), ct);
                    return user?.ToDto();
                },
                cancellationToken);

            if (userDto == null)
            {
                logger.LogWarning(
                    "User not found. CorrelationId: {CorrelationId}, UserId: {UserId}",
                    correlationId, query.UserId);

                return Result<UserDto>.Failure(Error.NotFound("User not found"));
            }

            logger.LogInformation(
                "User found successfully (cache hit/miss handled). CorrelationId: {CorrelationId}, UserId: {UserId}",
                correlationId, query.UserId);

            return Result<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve user by ID. CorrelationId: {CorrelationId}, UserId: {UserId}",
                correlationId, query.UserId);

            return Result<UserDto>.Failure(Error.Internal("Failed to retrieve user"));
        }
    }
}
