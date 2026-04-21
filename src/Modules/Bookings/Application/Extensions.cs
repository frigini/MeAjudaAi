using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Bookings.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services (commands, queries, etc) will be registered here
        return services;
    }
}
