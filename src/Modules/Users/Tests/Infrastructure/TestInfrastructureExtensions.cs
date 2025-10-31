using MeAjudaAi.Modules.Users.Application;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.Infrastructure;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure;

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

        // Adicionar serviços compartilhados essenciais (incluindo IDateTimeProvider)
        services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();

        // Usar extensões compartilhadas
        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        // Adicionar serviços de cache do Shared (incluindo ICacheService)
        // Para testes, usar implementação simples sem dependências complexas
        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, TestCacheService>();

        // Configurar banco de dados específico do módulo Users
        services.AddTestDatabase<UsersDbContext>(
            options.Database,
            "MeAjudaAi.Modules.Users.Infrastructure");

        // Configurar naming convention específica do Users
        services.PostConfigure<DbContextOptions<UsersDbContext>>(dbOptions =>
        {
            // Esta configuração específica será aplicada após a configuração genérica
        });

        // Configurar DbContext específico com snake_case naming
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
            .UseSnakeCaseNamingConvention() // Específico do Users
            .ConfigureWarnings(warnings =>
            {
                // Suprimir warnings de pending model changes em testes
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });
        });

        // Configurar mocks específicos do módulo Users
        services.AddUsersTestMocks(options.ExternalServices);

        // Adicionar repositórios específicos do Users
        services.AddScoped<IUserRepository, UserRepository>();

        // Adicionar serviços de aplicação (incluindo IUsersModuleApi)
        services.AddApplication();

        return services;
    }

    /// <summary>
    /// Adiciona mocks específicos do módulo Users
    /// </summary>
    private static IServiceCollection AddUsersTestMocks(
        this IServiceCollection services,
        TestExternalServicesOptions options)
    {
        if (options.UseKeycloakMock)
        {
            // Substituir serviços reais por mocks específicos do Users
            services.Replace(ServiceDescriptor.Scoped<IKeycloakService, MockKeycloakService>());
            services.Replace(ServiceDescriptor.Scoped<IUserDomainService, MockUserDomainService>());
            services.Replace(ServiceDescriptor.Scoped<IAuthenticationDomainService, MockAuthenticationDomainService>());
        }

        if (options.UseMessageBusMock)
        {
            // Usar mock compartilhado do message bus
            services.AddTestMessageBus();
        }

        return services;
    }
}

/// <summary>
/// Implementação de IDateTimeProvider para testes
/// </summary>
internal class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime CurrentDate() => DateTime.UtcNow;
}
