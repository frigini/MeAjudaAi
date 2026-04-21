using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestadores de serviços por cidade.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetProvidersByCityQuery(string City) : Query<Result<IReadOnlyList<ProviderDto>>>;
