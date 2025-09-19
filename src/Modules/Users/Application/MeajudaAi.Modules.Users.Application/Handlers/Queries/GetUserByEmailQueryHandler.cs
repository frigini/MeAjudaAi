using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Queries;

/// <summary>
/// Handler responsável por processar consultas de usuário por email.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para consultas específicas de usuário utilizando
/// o endereço de email como critério de busca. Útil para operações de login,
/// verificação de existência e recuperação de senha.
/// </remarks>
/// <param name="userRepository">Repositório para consultas de usuários</param>
/// <param name="logger">Logger para auditoria e rastreamento das operações</param>
internal sealed class GetUserByEmailQueryHandler(
    IUserRepository userRepository,
    ILogger<GetUserByEmailQueryHandler> logger
) : IQueryHandler<GetUserByEmailQuery, Result<UserDto>>
{
    /// <summary>
    /// Processa a consulta de usuário por email de forma assíncrona.
    /// </summary>
    /// <param name="query">Consulta contendo o email do usuário a ser buscado</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: UserDto com os dados do usuário encontrado
    /// - Falha: Mensagem "User not found" caso o usuário não exista
    /// </returns>
    /// <remarks>
    /// O processo é direto:
    /// 1. Busca o usuário pelo email no repositório
    /// 2. Verifica se o usuário existe
    /// 3. Converte para DTO se encontrado ou retorna erro
    /// 
    /// Utiliza value object Email para garantir type safety e validação.
    /// Muito utilizado em fluxos de autenticação e recuperação de conta.
    /// </remarks>
    public async Task<Result<UserDto>> HandleAsync(
        GetUserByEmailQuery query,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid();
        logger.LogInformation(
            "Starting user lookup by email. CorrelationId: {CorrelationId}, Email: {Email}",
            correlationId, query.Email);

        try
        {
            // Busca o usuário pelo email utilizando value object
            var user = await userRepository.GetByEmailAsync(
                new Email(query.Email), cancellationToken);

            if (user == null)
            {
                logger.LogWarning(
                    "User not found by email. CorrelationId: {CorrelationId}, Email: {Email}",
                    correlationId, query.Email);
                
                return Result<UserDto>.Failure("User not found");
            }

            logger.LogInformation(
                "User found successfully by email. CorrelationId: {CorrelationId}, UserId: {UserId}, Email: {Email}",
                correlationId, user.Id.Value, query.Email);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to retrieve user by email. CorrelationId: {CorrelationId}, Email: {Email}",
                correlationId, query.Email);
            
            return Result<UserDto>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }
}