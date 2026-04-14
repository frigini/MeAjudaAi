using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Handlers;
using MeAjudaAi.Modules.Ratings.Application.Services;
using MeAjudaAi.Shared.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MeAjudaAi.Modules.Ratings.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IContentModerator, ContentModerator>();
        services.AddScoped<ICommandHandler<CreateReviewCommand, Guid>, CreateReviewCommandHandler>();
        
        return services;
    }
}
