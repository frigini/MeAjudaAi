using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestadores de serviços por tipo.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetProvidersByTypeQuery(EProviderType Type) : Query<Result<IReadOnlyList<ProviderDto>>>;
