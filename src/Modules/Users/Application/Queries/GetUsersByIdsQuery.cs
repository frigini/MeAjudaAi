using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

/// <summary>
/// Query para buscar múltiplos usuários pelos seus identificadores únicos em uma única operação batch.
/// </summary>
/// <param name="UserIds">Lista de identificadores dos usuários a serem buscados</param>
/// <remarks>
/// Esta query implementa a otimização de batch queries para resolver o problema de N+1 queries
/// no método GetUsersBatchAsync. Em vez de executar N queries individuais, executa uma única
/// query batch usando WHERE IN na consulta SQL.
/// 
/// Benefícios:
/// - Reduz de N round-trips para 1 round-trip ao banco de dados
/// - Melhora significativamente a performance com listas grandes
/// - Permite otimizações de cache em lote
/// - Reduz contenção de recursos no pool de conexões
/// </remarks>
public record GetUsersByIdsQuery(IReadOnlyList<Guid> UserIds) : Query<Result<IReadOnlyList<UserDto>>>;
