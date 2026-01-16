using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;

public sealed record GetServiceByIdQuery(Guid Id)
    : Query<Result<ServiceDto?>>;
