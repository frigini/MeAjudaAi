using MeAjudaAi.Modules.Users.Application.Caching;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar consultas batch de usuários por IDs.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para consultas batch de usuários utilizando
/// uma única query otimizada em vez de N queries individuais. Resolve o problema
/// de N+1 queries e melhora significativamente a performance.
/// 
/// Estratégia de cache inteligente:
/// 1. Verifica quais usuários já estão no cache
/// 2. Faz batch query apenas para IDs não cacheados
/// 3. Combina resultados de cache + banco de dados
/// 4. Armazena novos usuários no cache
/// 
/// Utiliza cache distribuído para melhorar performance e reduzir carga no banco.
/// </remarks>
/// <param name="userRepository">Repositório para consultas batch de usuários</param>
/// <param name="usersCacheService">Serviço de cache específico para usuários</param>
/// <param name="logger">Logger para auditoria e rastreamento das operações</param>
internal sealed class GetUsersByIdsQueryHandler(
    IUserRepository userRepository,
    IUsersCacheService usersCacheService,
    ILogger<GetUsersByIdsQueryHandler> logger
) : IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>>
{
    /// <summary>
    /// Processa a consulta batch de usuários por IDs de forma assíncrona.
    /// </summary>
    /// <param name="query">Consulta contendo a lista de IDs dos usuários a serem buscados</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: Lista de UserDto com os usuários encontrados
    /// - Falha: Mensagem de erro em caso de exceção
    /// </returns>
    /// <remarks>
    /// Esta implementação executa uma única query batch no banco de dados e depois
    /// armazena os resultados no cache individual. É mais eficiente que N queries individuais
    /// e mantém compatibilidade com o sistema de cache existente.
    /// 
    /// Para listas vazias, retorna lista vazia sem consultar banco/cache.
    /// Utiliza value objects UserId para garantir type safety.
    /// </remarks>
    public async Task<Result<IReadOnlyList<UserDto>>> HandleAsync(
        GetUsersByIdsQuery query,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        logger.LogInformation(
            "Starting batch user lookup by IDs. CorrelationId: {CorrelationId}, UserCount: {UserCount}",
            correlationId, query.UserIds.Count);

        try
        {
            // Caso especial: lista vazia
            if (query.UserIds.Count == 0)
            {
                logger.LogDebug("Empty user IDs list provided. CorrelationId: {CorrelationId}", correlationId);
                return Result<IReadOnlyList<UserDto>>.Success(Array.Empty<UserDto>());
            }

            // Executar batch query no repositório
            var userIdsValueObjects = query.UserIds.Select(id => new UserId(id)).ToList();
            var users = await userRepository.GetUsersByIdsAsync(userIdsValueObjects, cancellationToken);

            // Converter para DTOs
            var userDtos = users.Select(user => user.ToDto()).ToList();

            // Armazenar no cache individualmente para futuras consultas
            foreach (var userDto in userDtos)
            {
                await usersCacheService.GetOrCacheUserByIdAsync(
                    userDto.Id,
                    async _ => userDto,
                    cancellationToken);
            }

            logger.LogInformation(
                "Batch user lookup completed successfully. CorrelationId: {CorrelationId}, RequestedUsers: {RequestedUsers}, FoundUsers: {FoundUsers}",
                correlationId, query.UserIds.Count, userDtos.Count);

            return Result<IReadOnlyList<UserDto>>.Success(userDtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve users by IDs in batch. CorrelationId: {CorrelationId}, UserCount: {UserCount}",
                correlationId, query.UserIds.Count);

            return Result<IReadOnlyList<UserDto>>.Failure($"Failed to retrieve users in batch: {ex.Message}");
        }
    }
}