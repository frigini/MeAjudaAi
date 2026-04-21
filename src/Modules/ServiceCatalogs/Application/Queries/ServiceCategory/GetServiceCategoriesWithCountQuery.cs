using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;

/// <summary>
/// Query to retrieve service categories with their service counts.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record GetServiceCategoriesWithCountQuery(bool ActiveOnly = false)
    : Query<Result<IReadOnlyList<ServiceCategoryWithCountDto>>>;
