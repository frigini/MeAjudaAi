using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestador de serviços por ID.
/// </summary>
public sealed record GetProviderByIdQuery(Guid ProviderId) : Query<Result<ProviderDto?>>;
