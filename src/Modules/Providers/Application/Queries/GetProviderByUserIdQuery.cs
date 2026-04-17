using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestador de serviços por ID do usuário.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetProviderByUserIdQuery(Guid UserId) : Query<Result<ProviderDto?>>;
