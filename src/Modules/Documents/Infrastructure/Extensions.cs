using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Modules.Documents.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Documents.Infrastructure.Jobs;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Queries;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddPersistence(configuration, environment);
        services.AddServices(configuration, environment);
        services.AddEventHandlers();
        services.AddJobs();

        services.ConfigureSchemaIsolation(configuration, ModuleNames.Documents, Schemas.Documents, DatabaseRoleConstants.Documents);

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? configuration.GetConnectionString("Documents")
                              ?? configuration.GetConnectionString("meajudaai-db");

        // Em ambientes de teste, permitir string de conexão placeholder pois os testes substituirão o DbContext
        var isTestEnvironment = EnvironmentHelpers.IsSecurityBypassEnvironment(environment);

        if (string.IsNullOrEmpty(connectionString))
        {
            if (isTestEnvironment)
            {
                // Usar placeholder para testes de integração - será substituído pela infraestrutura de testes
                connectionString = DatabaseConstants.DefaultTestConnectionString;
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
            if (environment.IsDevelopment())
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            }

            if (metricsInterceptor != null)
            {
                options.AddInterceptors(metricsInterceptor);
            }
        });

        // Unit of Work e Consultas
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.Documents, (sp, key) => sp.GetRequiredService<DocumentsDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<DocumentsDbContext>());
        
        // Repositories
        services.AddScoped<IRepository<Document, Guid>>(sp => sp.GetRequiredService<DocumentsDbContext>());

        services.AddScoped<IDocumentQueries, DbContextDocumentQueries>();
    }

    private static void AddServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
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

            // Azure Document Intelligence (OCR) - apenas se configurado
            services.AddScoped<IDocumentIntelligenceService, AzureDocumentIntelligenceService>();
        }

        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            // Azure Storage - apenas se configurado
            services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        }

        // Registrar implementações no-op como fallback apenas em ambientes de bypass (dev/test).
        // Em produção, a ausência das credenciais do Azure é um erro de configuração e causa
        // fail-fast para evitar que o serviço inicie sem dependências essenciais.
        if (EnvironmentHelpers.IsSecurityBypassEnvironment(environment))
        {
            services.TryAddScoped<IBlobStorageService, NullBlobStorageService>();
            services.TryAddScoped<IDocumentIntelligenceService, NullDocumentIntelligenceService>();
        }
        else
        {
            // Validação fail-fast: as implementações reais devem ter sido registradas acima.
            var registered = services.Any(sd => sd.ServiceType == typeof(IBlobStorageService));
            if (!registered)
                throw new InvalidOperationException(
                    "IBlobStorageService is not configured. Set 'Azure:Storage:ConnectionString' to enable file uploads.");

            var intelligenceRegistered = services.Any(sd => sd.ServiceType == typeof(IDocumentIntelligenceService));
            if (!intelligenceRegistered)
                throw new InvalidOperationException(
                    "IDocumentIntelligenceService is not configured. Set 'Azure:DocumentIntelligence:Endpoint' and 'Azure:DocumentIntelligence:ApiKey' to enable OCR.");
        }
    }

    private static void AddJobs(this IServiceCollection services)
    {
        // Document verification job
        services.AddScoped<IDocumentVerificationService, DocumentVerificationJob>();
    }

    /// <summary>
    /// Adiciona os Event Handlers do módulo Documents.
    /// </summary>
    private static void AddEventHandlers(this IServiceCollection services)
    {
        // Domain Event Handlers
        services.AddScoped<IEventHandler<DocumentVerifiedDomainEvent>, DocumentVerifiedDomainEventHandler>();
    }
}
