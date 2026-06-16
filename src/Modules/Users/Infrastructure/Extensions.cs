using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Queries;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Users.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços de infraestrutura do módulo Users.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços de infraestrutura do módulo Users.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddServices(configuration);
        services.AddEventHandlers();

        return services;
    }

    /// <summary>
    /// Configura a persistência do banco de dados e repositórios do módulo.
    /// </summary>
    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<UsersDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                  ?? configuration.GetConnectionString("Users")
                                  ?? configuration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrEmpty(connectionString))
            {
                var isIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "true";

                if (isIntegrationTests && EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
                {
                    // Fallback para testes de integração quando a flag INTEGRATION_TESTS=true é definida
                    connectionString = DatabaseConstants.DefaultTestConnectionString;
                }
                else
                {
                    throw new InvalidOperationException("Connection string for Users module not configured. Set INTEGRATION_TESTS=true for test environments.");
                }
            }

            // Obter interceptor de métricas se disponível
            var metricsInterceptor = serviceProvider.GetService<DatabaseMetricsInterceptor>();

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            if (environment.IsDevelopment())
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }

            // Adiciona interceptor de métricas se disponível
            if (metricsInterceptor != null)
            {
                options.AddInterceptors(metricsInterceptor);
            }
        });

        // AUTO-MIGRATION: Configura factory para auto-aplicar migrations quando necessário
        services.AddScoped<Func<UsersDbContext>>(provider => () =>
        {
            var context = provider.GetRequiredService<UsersDbContext>();
            // Aplica migrações pendentes - ABORDAGEM PREGUIÇOSA
            context.Database.Migrate();
            return context;
        });

        // Registra processador de eventos de domínio
        services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();

        // Unit of Work e Repositórios
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Users, (sp, key) => sp.GetRequiredService<UsersDbContext>());

        services.AddScoped<IRepository<User, Guid>>(sp => sp.GetRequiredService<UsersDbContext>());

        // Consultas otimizadas
        services.AddScoped<IUserQueries, DbContextUserQueries>();
    }

    /// <summary>
    /// Configura os serviços de autenticação e domínio do módulo.
    /// </summary>
    private static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Sempre registra as opções do Keycloak
        services.AddSingleton(provider =>
        {
            var options = new KeycloakOptions();
            configuration.GetSection(KeycloakOptions.SectionName).Bind(options);
            return options;
        });

        var shouldUseMock = ShouldUseMockKeycloakServices(configuration);

        if (!shouldUseMock)
        {
            // Registra serviço real quando Keycloak está habilitado e configurado
            services.AddHttpClient<IKeycloakService, KeycloakService>();
            services.AddScoped<IUserDomainService, KeycloakUserDomainService>();
            services.AddScoped<IAuthenticationDomainService, KeycloakAuthenticationDomainService>();
        }
        else
        {
            // Registra implementações de desenvolvimento local quando Keycloak não está disponível
            services.AddScoped<IUserDomainService, LocalDevelopmentUserDomainService>();
            services.AddScoped<IAuthenticationDomainService, LocalDevelopmentAuthenticationDomainService>();
        }
    }

    /// <summary>
    /// Determina se serviços mock do Keycloak devem ser usados com base na configuração.
    /// </summary>
    private static bool ShouldUseMockKeycloakServices(IConfiguration configuration)
    {
        var keycloakEnabledString = configuration["Keycloak:Enabled"] ?? configuration["Keycloak__Enabled"];
        var keycloakEnabled = !string.Equals(keycloakEnabledString, "false", StringComparison.OrdinalIgnoreCase);

        var keycloakSection = configuration.GetSection("Keycloak");
        var hasValidKeycloakConfig = !string.IsNullOrEmpty(keycloakSection["BaseUrl"]) &&
                                   !string.IsNullOrEmpty(keycloakSection["Realm"]) &&
                                   !string.IsNullOrEmpty(keycloakSection["ClientId"]) &&
                                   !string.IsNullOrEmpty(keycloakSection["ClientSecret"]);

        return !keycloakEnabled || !hasValidKeycloakConfig;
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Users.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IEventHandler<UserRegisteredDomainEvent>, UserRegisteredDomainEventHandler>();
        services.AddScoped<IEventHandler<UserProfileUpdatedDomainEvent>, UserProfileUpdatedDomainEventHandler>();
        services.AddScoped<IEventHandler<UserDeletedDomainEvent>, UserDeletedDomainEventHandler>();
    }
}
