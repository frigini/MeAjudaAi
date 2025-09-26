using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Users.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddKeycloak(configuration);
        services.AddDomainServices();
        services.AddEventHandlers();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Usa PostgreSQL para todos os ambientes (TestContainers fornecerá database de teste)
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                              ?? configuration.GetConnectionString("Users")
                              ?? configuration.GetConnectionString("meajudaai-db");

        services.AddDbContext<UsersDbContext>((serviceProvider, options) =>
        {
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
            
            // Adiciona interceptor de métricas se disponível
            if (metricsInterceptor != null)
            {
                options.AddInterceptors(metricsInterceptor);
            }
        });

        // AUTO-MIGRATION: Configura factory para auto-criar banco de dados quando necessário
        services.AddScoped<Func<UsersDbContext>>(provider => () =>
        {
            var context = provider.GetRequiredService<UsersDbContext>();
            // Garante que o banco de dados existe - ABORDAGEM PREGUIÇOSA
            context.Database.EnsureCreated();
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
        // Registro direto da configuração do Keycloak
        services.AddSingleton(provider =>
        {
            var options = new KeycloakOptions();
            configuration.GetSection(KeycloakOptions.SectionName).Bind(options);
            return options;
        });

        // Verifica se Keycloak está habilitado para usar implementação real ou mock
        var keycloakEnabledString = configuration["Keycloak:Enabled"];
        var keycloakEnabled = !string.Equals(keycloakEnabledString, "false", StringComparison.OrdinalIgnoreCase);
        
        if (keycloakEnabled)
        {
            services.AddHttpClient<IKeycloakService, KeycloakService>();
        }

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Domain Services específicos do módulo Users
        services.AddScoped<IUserDomainService, KeycloakUserDomainService>();
        services.AddScoped<IAuthenticationDomainService, KeycloakAuthenticationDomainService>();

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