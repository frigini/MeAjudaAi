using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A string de conexão 'DefaultConnection' não foi configurada para o módulo de Ratings.");
        }

        services.AddDbContext<RatingsDbContext>(options =>
            options.UseNpgsql(connectionString, m => m.MigrationsHistoryTable("__EFMigrationsHistory", "ratings")));

        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }
}
