using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Ratings.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString) && !MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
        {
            throw new InvalidOperationException("A string de conexão 'DefaultConnection' não foi configurada para o módulo de Ratings.");
        }

        services.AddDbContext<RatingsDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseNpgsql(connectionString, m => m.MigrationsHistoryTable("__EFMigrationsHistory", "ratings"));
            }
        });

        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }
}
