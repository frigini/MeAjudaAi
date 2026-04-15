using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<RatingsDbContext>((serviceProvider, options) =>
        {
            var resolvedConfig = serviceProvider.GetRequiredService<IConfiguration>();
            var connStr = resolvedConfig.GetConnectionString("DefaultConnection") ?? 
                          resolvedConfig.GetConnectionString("Ratings") ??
                          resolvedConfig.GetConnectionString("meajudaai-db");

            if (string.IsNullOrWhiteSpace(connStr) && MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
            {
#pragma warning disable S2068
                connStr = "Host=localhost;Port=5432;Database=meajudaai_test;Username=postgres;Password=test";
#pragma warning restore S2068
            }

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                options.UseNpgsql(connStr, m => m.MigrationsHistoryTable("__EFMigrationsHistory", "ratings"));
            }
        });

        services.AddScoped<IReviewRepository, ReviewRepository>();

        return services;
    }
}
