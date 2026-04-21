using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;

[ExcludeFromCodeCoverage]

public sealed record GetAllServiceCategoriesQuery(bool ActiveOnly = false)
    : Query<Result<IReadOnlyList<ServiceCategoryDto>>>;
