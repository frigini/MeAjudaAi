using MeAjudaAi.Modules.Ratings.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IContentModerator, ContentModerator>();
        
        return services;
    }
}
