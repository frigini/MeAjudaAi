using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure;

public static class LocationsTestInfrastructureExtensions
{
    public static IServiceCollection AddLocationsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions options)
    {
        services.AddDbContext<LocationsDbContext>(dbOptions =>
        {
            dbOptions.UseNpgsql(SharedTestContainers.PostgreSql.GetConnectionString(), npgsqlOptions =>
            {
                if (!string.IsNullOrWhiteSpace(options.Database.Schema))
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                }
            })
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LocationsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Locations,
            (sp, key) => sp.GetRequiredService<LocationsDbContext>());

        services.AddScoped<IRepository<AllowedCity, Guid>>(sp => sp.GetRequiredService<LocationsDbContext>());
        services.AddScoped<IAllowedCityQueries, DbContextAllowedCityQueries>();

        services.AddLocalization();
        services.AddLogging();

        return services;
    }
}
