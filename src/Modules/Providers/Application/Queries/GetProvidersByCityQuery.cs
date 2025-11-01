using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestadores de serviços por cidade.
/// </summary>
public sealed record GetProvidersByCityQuery(string City) : Query<Result<IReadOnlyList<ProviderDto>>>;
