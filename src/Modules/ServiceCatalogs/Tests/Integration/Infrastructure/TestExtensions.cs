using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration.Infrastructure;

public static class TestExtensions
{
    public static IServiceCollection AddServiceCatalogsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        services.AddCommonModuleTestInfrastructure<ServiceCatalogsDbContext>(
            options,
            migrationsAssembly: typeof(ServiceCatalogsDbContext).Assembly.FullName,
            configureDbContext: dbOptions => dbOptions.UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.ServiceCatalogs, (sp, key) => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IServiceCategoryQueries, DbContextServiceCategoryQueries>();
        services.AddScoped<IServiceQueries, DbContextServiceQueries>();

        if (options?.ExternalServices?.UseMessageBusMock == true)
        {
            services.AddTestMessageBus();
        }

        services.AddApplication();

        return services;
    }
}
