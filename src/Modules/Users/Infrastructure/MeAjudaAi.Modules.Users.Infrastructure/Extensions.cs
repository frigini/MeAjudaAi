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
                
                // PERFORMANCE: Timeout mais longo para permitir criação de database
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

        // AUTO-MIGRATION: Configura factory para auto-criar database quando necessário
        services.AddScoped<Func<UsersDbContext>>(provider => () =>
        {
            var context = provider.GetRequiredService<UsersDbContext>();
            // Garante que database existe - LAZY APPROACH
            context.Database.EnsureCreated();
            return context;
        });

        // Registra processador de eventos de domínio (abordagem de injeção de dependência direta)
        services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();

        // Repositories - atualmente só há um, mas pode ser expandido com Scrutor no futuro
        services.AddScoped<IUserRepository, UserRepository>();
        
        // Quando houver mais repositories, pode usar Scrutor:
        // services.Scan(scan => scan
        //     .FromCallingAssembly()
        //     .AddClasses(classes => classes.Where(type => 
        //         type.Name.EndsWith("Repository") && !type.IsInterface))
        //     .AsImplementedInterfaces()
        //     .WithScopedLifetime());

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
        else
        {
            services.AddScoped<IKeycloakService, MockKeycloakService>();
        }

        return services;
    }

    private static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Registro manual específico para Domain Services que não seguem convenções
        services.AddScoped<IUserDomainService, KeycloakUserDomainService>();
        services.AddScoped<IAuthenticationDomainService, KeycloakAuthenticationDomainService>();

        // Exemplo de como usar Scrutor para registrar serviços por convenção:
        // services.Scan(scan => scan
        //     .FromCallingAssembly()
        //     .AddClasses(classes => classes.Where(type => type.Name.EndsWith("DomainService")))
        //     .AsImplementedInterfaces()
        //     .WithScopedLifetime());

        return services;
    }

    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        // REMOVED: Event Handlers são registrados automaticamente pelo Scrutor no Shared
        // O Scrutor já faz isso através de:
        // - services.Scan(...).AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
        
        // Se houver Event Handlers específicos que não seguem o padrão, registre-os aqui
        
        return services;
    }
}