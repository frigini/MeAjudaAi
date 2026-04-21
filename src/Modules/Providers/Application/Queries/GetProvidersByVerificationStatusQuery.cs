using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.Queries;

/// <summary>
/// Query para buscar prestadores de serviços por status de verificação.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetProvidersByVerificationStatusQuery(EVerificationStatus Status) : Query<Result<IReadOnlyList<ProviderDto>>>;
