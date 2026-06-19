using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Modules.Ratings.Application.Handlers;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Idempotency;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Ratings.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços de infraestrutura do módulo Ratings.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra todos os serviços de infraestrutura do módulo Ratings.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddEventHandlers();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Ratings, Schemas.Ratings, Schemas.Roles.Ratings);

        return services;
    }

    /// <summary>
    /// Configura a persistência do banco de dados e repositórios do módulo.
    /// </summary>
    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<RatingsDbContext>((serviceProvider, options) =>
        {
            var resolvedConfig = serviceProvider.GetRequiredService<IConfiguration>();
            var connStr = resolvedConfig.GetConnectionString("DefaultConnection") ??
                          resolvedConfig.GetConnectionString("Ratings") ??
                          resolvedConfig.GetConnectionString("meajudaai-db");

            if (string.IsNullOrWhiteSpace(connStr) && EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
            {
                connStr = DatabaseConstants.DefaultTestConnectionString;
            }

            if (!string.IsNullOrWhiteSpace(connStr))
            {
                options.UseNpgsql(connStr, m => m.MigrationsHistoryTable("__EFMigrationsHistory", "ratings"));
            }
        });

        // Unit of Work e Repositórios
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<RatingsDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Ratings, (sp, key) => sp.GetRequiredService<RatingsDbContext>());

        services.AddScoped<IIdempotencyRepository>(sp => new RatingsIdempotencyRepository(sp.GetRequiredService<RatingsDbContext>()));
        
        services.AddScoped<IRepository<Review, ReviewId>>(sp => sp.GetRequiredService<RatingsDbContext>());

        // Consultas otimizadas
        services.AddScoped<IReviewQueries, DbContextReviewQueries>();

        // Command Handlers
        services.AddScoped<CreateReviewCommandHandler>();
        services.AddScoped<ICommandHandler<CreateReviewCommand, Guid>>(sp => sp.GetRequiredService<CreateReviewCommandHandler>());
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Ratings.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        // Domain Event Handlers
        services.AddScoped<IEventHandler<ReviewApprovedDomainEvent>, ReviewApprovedDomainEventHandler>();
        services.AddScoped<IEventHandler<ReviewRejectedDomainEvent>, ReviewRejectedDomainEventHandler>();

        // Integration Event Handlers
        services.AddScoped<IEventHandler<UserDeletedIntegrationEvent>, UserDeletedIntegrationEventHandler>();
    }
}
