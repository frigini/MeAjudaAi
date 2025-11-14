using EFCore.NamingConventions;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Jobs;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using MeAjudaAi.Shared.Database;
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
        services.AddJobs();

        return services;
    }

    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? configuration.GetConnectionString("Documents")
                              ?? configuration.GetConnectionString("meajudaai-db");

        services.AddDbContext<DocumentsDbContext>((serviceProvider, options) =>
        {
            var metricsInterceptor = serviceProvider.GetService<DatabaseMetricsInterceptor>();

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Documents.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "meajudaai_documents");
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .EnableServiceProviderCaching()
            .EnableSensitiveDataLogging(false);

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
        // Verificar se está em ambiente de teste
        var isTestEnvironment = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "true"
                               || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";

        if (isTestEnvironment)
        {
            // Usar mocks em ambiente de teste
            services.AddScoped<IBlobStorageService, MockBlobStorageService>();
            services.AddScoped<IDocumentIntelligenceService, MockDocumentIntelligenceService>();
        }
        else
        {
            // Registrar Azure clients para produção
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
            }

            // Azure Storage
            services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

            // Azure Document Intelligence (OCR)
            services.AddScoped<IDocumentIntelligenceService, AzureDocumentIntelligenceService>();
        }

        return services;
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        // Document verification job
        services.AddScoped<IDocumentVerificationService, DocumentVerificationJob>();

        return services;
    }
}
