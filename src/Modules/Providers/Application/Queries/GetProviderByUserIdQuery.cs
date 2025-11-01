using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestador de serviços por ID do usuário.
/// </summary>
public sealed record GetProviderByUserIdQuery(Guid UserId) : Query<Result<ProviderDto?>>;
