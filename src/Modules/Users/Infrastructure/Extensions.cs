using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Users.Infrastructure;

public static class Extensions
{
    /// <summary>
    /// Registra serviços de infraestrutura do módulo Users incluindo persistência, integração com Keycloak, serviços de domínio e manipuladores de eventos.
    /// </summary>
    /// <param name="services">A coleção de serviços a ser configurada.</param>
    /// <param name="configuration">A configuração da aplicação contendo configurações de banco de dados e Keycloak.</param>
    /// <returns>A coleção de serviços configurada para encadeamento fluente.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddKeycloak(configuration);
        services.AddDomainServices(configuration);
        services.AddEventHandlers();

        return services;
    }

    /// <summary>
    /// Determina se serviços mock do Keycloak devem ser usados com base na configuração.
    /// Verifica se o Keycloak está explicitamente desabilitado ou se a configuração necessária está faltando.
    /// </summary>
    private static bool ShouldUseMockKeycloakServices(IConfiguration configuration)
    {
        // Verifica se Keycloak está habilitado - verifica múltiplas variações da configuração
        var keycloakEnabledString = configuration["Keycloak:Enabled"] ?? configuration["Keycloak__Enabled"];
        var keycloakEnabled = !string.Equals(keycloakEnabledString, "false", StringComparison.OrdinalIgnoreCase);

        // Verifica também se existe uma seção de configuração do Keycloak com valores válidos
        var keycloakSection = configuration.GetSection("Keycloak");
        var hasValidKeycloakConfig = !string.IsNullOrEmpty(keycloakSection["BaseUrl"]) &&
                                   !string.IsNullOrEmpty(keycloakSection["Realm"]) &&
                                   !string.IsNullOrEmpty(keycloakSection["ClientId"]) &&
                                   !string.IsNullOrEmpty(keycloakSection["ClientSecret"]);

        // Se Keycloak está explicitamente desabilitado OU não há configuração válida, usa mock
        return !keycloakEnabled || !hasValidKeycloakConfig;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>((serviceProvider, options) =>
        {
            // Usa PostgreSQL para todos os ambientes (TestContainers fornecerá database de teste)
            // Resolve a string de conexão a partir da configuração do DI, incluindo sobrescritas de testes
            var resolvedConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = resolvedConfiguration.GetConnectionString("DefaultConnection")
                                  ?? resolvedConfiguration.GetConnectionString("Users")
                                  ?? resolvedConfiguration.GetConnectionString("meajudaai-db");

            if (string.IsNullOrEmpty(connectionString))
            {
                var env = serviceProvider.GetService<IHostEnvironment>();
                var isIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "true";
                
                if (isIntegrationTests && MeAjudaAi.Shared.Utilities.EnvironmentHelpers.IsSecurityBypassEnvironment(env))
                {
                    // Fallback para testes de integração quando a flag INTEGRATION_TESTS=true é definida
#pragma warning disable S2068 // "password" detected here, make sure this is not a hard-coded credential
                    connectionString = MeAjudaAi.Shared.Database.DatabaseConstants.DefaultTestConnectionString;
#pragma warning restore S2068
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

                // PERFORMANCE: Timeout mais longo para permitir criação do banco de dados
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            // Configurações consistentes para evitar problemas com compiled queries
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            var environment = serviceProvider.GetService<IHostEnvironment>();
            if (environment?.IsDevelopment() == true)
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

        // Registra processador de eventos de domínio (abordagem de injeção de dependência direta)
        services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    private static IServiceCollection AddKeycloak(this IServiceCollection services, IConfiguration configuration)
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
        }
        // Quando shouldUseMock é true, não registra nada (será necessário configurar manualmente para testes)

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        var shouldUseMock = ShouldUseMockKeycloakServices(configuration);

        if (!shouldUseMock)
        {
            // Registra serviços reais quando Keycloak está habilitado e configurado
            services.AddScoped<IUserDomainService, KeycloakUserDomainService>();
            services.AddScoped<IAuthenticationDomainService, KeycloakAuthenticationDomainService>();
        }
        else
        {
            // Registra implementações de desenvolvimento local quando Keycloak não está disponível ou configurado
            services.AddScoped<IUserDomainService, LocalDevelopmentUserDomainService>();
            services.AddScoped<IAuthenticationDomainService, LocalDevelopmentAuthenticationDomainService>();
        }

        return services;
    }

    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        // Event Handlers específicos do módulo Users
        services.AddScoped<IEventHandler<UserRegisteredDomainEvent>, UserRegisteredDomainEventHandler>();
        services.AddScoped<IEventHandler<UserProfileUpdatedDomainEvent>, UserProfileUpdatedDomainEventHandler>();
        services.AddScoped<IEventHandler<UserDeletedDomainEvent>, UserDeletedDomainEventHandler>();

        return services;
    }
}
