using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;

/// <summary>
/// Query to retrieve a service category by its identifier.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetServiceCategoryByIdQuery(Guid Id)
    : Query<Result<ServiceCategoryDto?>>;
