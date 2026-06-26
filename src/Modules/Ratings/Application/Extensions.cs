using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Handlers.Queries;
using MeAjudaAi.Modules.Ratings.Application.ModuleApi;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.Application;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRatingsModuleApi, RatingsModuleApi>();
        services.AddScoped<IContentModerator, ContentModerator>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetReviewByIdQuery, Result<ProviderReviewResponse>>, GetReviewByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetProviderReviewsQuery, Result<PagedResult<ProviderReviewResponse>>>, GetProviderReviewsQueryHandler>();
        services.AddScoped<IQueryHandler<GetReviewStatusQuery, Result<ReviewStatusResponse>>, GetReviewStatusQueryHandler>();

        return services;
    }
}
