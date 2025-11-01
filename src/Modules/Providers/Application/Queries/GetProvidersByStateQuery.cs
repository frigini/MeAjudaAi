using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestadores de serviços por estado.
/// </summary>
public sealed record GetProvidersByStateQuery(string State) : Query<Result<IReadOnlyList<ProviderDto>>>;
