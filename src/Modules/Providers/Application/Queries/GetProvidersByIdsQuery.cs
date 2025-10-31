using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar múltiplos prestadores de serviços por IDs.
/// </summary>
public sealed record GetProvidersByIdsQuery(IReadOnlyList<Guid> ProviderIds) : Query<Result<IReadOnlyList<ProviderDto>>>;