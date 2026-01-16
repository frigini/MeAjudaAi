using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar consultas paginadas de usuários.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para consultas de listagem de usuários com suporte
/// à paginação. Retorna uma lista paginada de usuários convertidos para DTOs,
/// otimizando performance e experiência do usuário em grandes volumes de dados.
/// </remarks>
/// <param name="userRepository">Repositório para consultas de usuários</param>
/// <param name="logger">Logger para auditoria e rastreamento das operações</param>
internal sealed class GetUsersQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUsersQueryHandler> logger
) : IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    /// <summary>
    /// Processa a consulta de usuários paginada de forma assíncrona.
    /// </summary>
    /// <param name="query">Consulta contendo parâmetros de paginação (página e tamanho da página)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: PagedResult com lista de UserDto e metadados de paginação
    /// - Falha: Mensagem de erro descritiva
    /// </returns>
    /// <remarks>
    /// O processo inclui:
    /// 1. Consulta paginada ao repositório
    /// 2. Conversão das entidades para DTOs
    /// 3. Criação do resultado paginado com metadados
    /// 4. Retorno do resultado encapsulado
    /// 
    /// Todas as exceções são capturadas e convertidas em resultados de falha.
    /// </remarks>
    public async Task<Result<PagedResult<UserDto>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetUsersQuery",
            ["Page"] = query.Page,
            ["PageSize"] = query.PageSize
        });

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        logger.LogInformation("Starting paginated user listing for page {Page}, size {PageSize}",
            query.Page, query.PageSize);

        try
        {
            // Validar parâmetros de paginação
            var validationResult = ValidatePaginationParameters(query);
            if (validationResult.IsFailure)
                return Result<PagedResult<UserDto>>.Failure(validationResult.Error);

            // Executar consulta paginada
            var (users, totalCount) = await ExecutePagedQueryAsync(query, stopwatch, cancellationToken);

            // Mapear entidades para DTOs
            var userDtos = MapUsersToDto(users, stopwatch);

            // Criar resultado paginado
            var pagedResult = CreatePagedResult(userDtos, query, totalCount);

            stopwatch.Stop();
            logger.LogInformation(
                "Paginated user listing completed successfully in {ElapsedMs}ms - TotalCount: {TotalCount}, ReturnedCount: {ReturnedCount}, Page: {Page}/{TotalPages}",
                stopwatch.ElapsedMilliseconds, totalCount, userDtos.Count, query.Page, pagedResult.TotalPages);

            return Result<PagedResult<UserDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "Failed to retrieve paginated users after {ElapsedMs}ms - Page: {Page}, PageSize: {PageSize}",
                stopwatch.ElapsedMilliseconds, query.Page, query.PageSize);

            return Result<PagedResult<UserDto>>.Failure($"Failed to retrieve users: {ex.Message}");
        }
    }

    /// <summary>
    /// Valida os parâmetros de paginação fornecidos na consulta.
    /// </summary>
    private Result<Unit> ValidatePaginationParameters(GetUsersQuery query)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > 100)
        {
            logger.LogWarning("Invalid pagination parameters: Page={Page}, PageSize={PageSize}",
                query.Page, query.PageSize);
            return Result<Unit>.Failure("Invalid pagination parameters");
        }

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Executa a consulta paginada no repositório.
    /// </summary>
    private async Task<(IReadOnlyList<Domain.Entities.User> users, int totalCount)> ExecutePagedQueryAsync(
        GetUsersQuery query,
        System.Diagnostics.Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing repository query for users");

        var repositoryStart = stopwatch.ElapsedMilliseconds;
        var (users, totalCount) = await userRepository.GetPagedAsync(
            query.Page, query.PageSize, cancellationToken);

        logger.LogDebug("Repository query completed in {ElapsedMs}ms, found {TotalCount} total users",
            stopwatch.ElapsedMilliseconds - repositoryStart, totalCount);

        return (users, totalCount);
    }

    /// <summary>
    /// Mapeia as entidades de usuário para DTOs.
    /// </summary>
    private IReadOnlyList<UserDto> MapUsersToDto(
        IReadOnlyList<Domain.Entities.User> users,
        System.Diagnostics.Stopwatch stopwatch)
    {
        var mappingStart = stopwatch.ElapsedMilliseconds;
        var userDtos = users.Select(u => u.ToDto()).ToList().AsReadOnly();

        logger.LogDebug("DTO mapping completed in {ElapsedMs}ms for {UserCount} users",
            stopwatch.ElapsedMilliseconds - mappingStart, userDtos.Count);

        return userDtos;
    }

    /// <summary>
    /// Cria o resultado paginado com metadados.
    /// </summary>
    private static PagedResult<UserDto> CreatePagedResult(
        IReadOnlyList<UserDto> userDtos,
        GetUsersQuery query,
        int totalCount)
    {
        return PagedResult<UserDto>.Create(
            userDtos, query.Page, query.PageSize, totalCount);
    }
}
