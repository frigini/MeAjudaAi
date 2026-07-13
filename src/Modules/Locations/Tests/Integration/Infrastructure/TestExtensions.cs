using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Locations.Tests.Integration.Infrastructure;

public static class LocationsTestInfrastructureExtensions
{
    public static IServiceCollection AddLocationsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions options)
    {
        services.AddCommonModuleTestInfrastructure<LocationsDbContext>(
            options,
            configureDbContext: dbOptions =>
            {
                dbOptions.UseSnakeCaseNamingConvention();
                dbOptions.EnableSensitiveDataLogging();
                dbOptions.EnableDetailedErrors();
            });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LocationsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Locations,
            (sp, key) => sp.GetRequiredService<LocationsDbContext>());

        services.AddScoped<IRepository<AllowedCity, Guid>>(sp => sp.GetRequiredService<LocationsDbContext>());
        services.AddScoped<IAllowedCityQueries, DbContextAllowedCityQueries>();

        return services;
    }
}
