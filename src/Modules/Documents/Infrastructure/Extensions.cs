using EFCore.NamingConventions;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Documents.Infrastructure.Jobs;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Documents.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddServices(configuration);
        services.AddEventHandlers();
        services.AddJobs();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? configuration.GetConnectionString("Documents")
                              ?? configuration.GetConnectionString("meajudaai-db");

        // In test environments, allow placeholder connection string since tests will replace the DbContext
        var isTestEnvironment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Testing", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrEmpty(connectionString))
        {
            if (isTestEnvironment)
            {
                // Use placeholder for integration tests - will be replaced by test infrastructure
                connectionString = "Host=localhost;Database=test;Username=test;Password=test";
            }
            else
            {
                throw new InvalidOperationException(
                    "Database connection string is not configured. "
                    + "Please set one of the following configuration keys: "
                    + "'ConnectionStrings:DefaultConnection', 'ConnectionStrings:Documents', or 'ConnectionStrings:meajudaai-db'");
            }
        }

        services.AddDbContext<DocumentsDbContext>((serviceProvider, options) =>
        {
            var metricsInterceptor = serviceProvider.GetService<DatabaseMetricsInterceptor>();

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Documents.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

            // Suprimir o warning PendingModelChangesWarning apenas em ambiente de desenvolvimento
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var isDevelopment = string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);
            
            if (isDevelopment)
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }

            if (metricsInterceptor != null)
            {
                options.AddInterceptors(metricsInterceptor);
            }
        });

        // Repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Registrar Azure clients
        var storageConnectionString = configuration["Azure:Storage:ConnectionString"];
        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            services.AddSingleton(sp =>
                new Azure.Storage.Blobs.BlobServiceClient(storageConnectionString));
        }

        var documentIntelligenceEndpoint = configuration["Azure:DocumentIntelligence:Endpoint"];
        var documentIntelligenceApiKey = configuration["Azure:DocumentIntelligence:ApiKey"];
        if (!string.IsNullOrEmpty(documentIntelligenceEndpoint) && !string.IsNullOrEmpty(documentIntelligenceApiKey))
        {
            services.AddSingleton(sp =>
                new Azure.AI.DocumentIntelligence.DocumentIntelligenceClient(
                    new Uri(documentIntelligenceEndpoint),
                    new Azure.AzureKeyCredential(documentIntelligenceApiKey)));

            // Azure Document Intelligence (OCR) - only if configured
            services.AddScoped<IDocumentIntelligenceService, AzureDocumentIntelligenceService>();
        }

        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            // Azure Storage - only if configured
            services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        }

        return services;
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        // Document verification job
        services.AddScoped<IDocumentVerificationService, DocumentVerificationJob>();

        return services;
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Documents.
    /// </summary>
    private static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        // Domain Event Handlers
        services.AddScoped<IEventHandler<DocumentVerifiedDomainEvent>, DocumentVerifiedDomainEventHandler>();

        return services;
    }
}
