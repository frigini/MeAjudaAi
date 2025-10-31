using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar consultas de usuário por username.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para consultas específicas de usuário utilizando
/// o username como critério de busca. Útil para operações de login,
/// verificação de existência e busca por nome de usuário único.
/// </remarks>
/// <param name="userRepository">Repositório para consultas de usuários</param>
/// <param name="logger">Logger para auditoria e rastreamento das operações</param>
internal sealed class GetUserByUsernameQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserByUsernameQueryHandler> logger
) : IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>
{
    /// <summary>
    /// Processa a consulta de usuário por username de forma assíncrona.
    /// </summary>
    /// <param name="query">Consulta contendo o username do usuário a ser buscado</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: UserDto com os dados do usuário encontrado
    /// - Falha: Mensagem "User not found" caso o usuário não exista
    /// </returns>
    /// <remarks>
    /// O processo é direto:
    /// 1. Busca o usuário pelo username no repositório
    /// 2. Verifica se o usuário existe
    /// 3. Converte para DTO se encontrado ou retorna erro
    /// 
    /// Utiliza value object Username para garantir type safety e validação.
    /// Muito utilizado em fluxos de autenticação e verificação de unicidade.
    /// </remarks>
    public async Task<Result<UserDto>> HandleAsync(
        GetUserByUsernameQuery query,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        logger.LogInformation(
            "Starting user lookup by username. CorrelationId: {CorrelationId}, Username: {Username}",
            correlationId, query.Username);

        try
        {
            // Busca o usuário pelo username utilizando value object
            var user = await userRepository.GetByUsernameAsync(
                new Username(query.Username), cancellationToken);

            if (user == null)
            {
                logger.LogWarning(
                    "User not found by username. CorrelationId: {CorrelationId}, Username: {Username}",
                    correlationId, query.Username);

                return Result<UserDto>.Failure(Error.NotFound(ValidationMessages.NotFound.User));
            }

            logger.LogInformation(
                "User found successfully by username. CorrelationId: {CorrelationId}, UserId: {UserId}, Username: {Username}",
                correlationId, user.Id.Value, query.Username);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve user by username. CorrelationId: {CorrelationId}, Username: {Username}",
                correlationId, query.Username);

            return Result<UserDto>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }
}
