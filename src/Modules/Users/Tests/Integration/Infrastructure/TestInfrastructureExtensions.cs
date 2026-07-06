using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Users;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Users.Tests.Integration.Infrastructure;

/// <summary>
/// Extensões para configurar infraestrutura de testes específica do módulo Users
/// </summary>
public static class UsersTestInfrastructureExtensions
{
    /// <summary>
    /// Adiciona toda a infraestrutura de testes necessária para o módulo Users
    /// </summary>
    public static IServiceCollection AddUsersTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);

        services.AddSingleton(TimeProvider.System);

        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        services.AddSingleton<ICacheService, TestCacheService>();

        services.AddTestDatabase<UsersDbContext>(
            options.Database,
            "MeAjudaAi.Modules.Users.Infrastructure");

        services.PostConfigure<DbContextOptions<UsersDbContext>>(dbOptions =>
        {
        });

        services.AddDbContext<UsersDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
            var connectionString = container.GetConnectionString();

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
            {
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });
        });

        services.AddUsersTestMocks(options.ExternalServices);

        services.AddScoped<MeAjudaAi.Shared.Database.Abstractions.IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());
        services.AddScoped<IUserQueries, MeAjudaAi.Modules.Users.Infrastructure.Queries.DbContextUserQueries>();

        services.AddApplication();

        return services;
    }

    private static IServiceCollection AddUsersTestMocks(
        this IServiceCollection services,
        TestExternalServicesOptions options)
    {
        if (options.UseKeycloakMock)
        {
            services.Replace(ServiceDescriptor.Scoped<IKeycloakService, MockKeycloakService>());
            services.Replace(ServiceDescriptor.Scoped<IUserDomainService, MockUserDomainService>());
            services.Replace(ServiceDescriptor.Scoped<IAuthenticationDomainService, MockAuthenticationDomainService>());
        }

        if (options.UseMessageBusMock)
        {
            services.AddTestMessageBus();
        }

        return services;
    }
}
