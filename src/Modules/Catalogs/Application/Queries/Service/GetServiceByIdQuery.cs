using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Catalogs.Application.Queries.Service;

public sealed record GetServiceByIdQuery(Guid Id)
    : Query<Result<ServiceDto?>>;
