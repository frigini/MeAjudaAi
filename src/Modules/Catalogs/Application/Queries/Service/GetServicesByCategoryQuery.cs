using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Queries.Service;

public sealed record GetServicesByCategoryQuery(Guid CategoryId, bool ActiveOnly = false)
    : Query<Result<IReadOnlyList<ServiceListDto>>>;
