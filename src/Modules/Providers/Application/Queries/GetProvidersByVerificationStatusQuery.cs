using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestadores de serviços por status de verificação.
/// </summary>
public sealed record GetProvidersByVerificationStatusQuery(EVerificationStatus Status) : Query<Result<IReadOnlyList<ProviderDto>>>;