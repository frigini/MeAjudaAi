using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestadores de serviços por tipo.
/// </summary>
public sealed record GetProvidersByTypeQuery(EProviderType Type) : Query<Result<IReadOnlyList<ProviderDto>>>;
