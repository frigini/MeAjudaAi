using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.Storage.Blobs;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.BackgroundServices;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using MeAjudaAi.Shared.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Documents.API;

public static class DocumentsModule
{
    public static IServiceCollection AddDocumentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<DocumentsDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "meajudaai_documents");
            });
        });

        // Repositories
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // MediatR Handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<UploadDocumentCommandHandler>();
        });

        // Azure Services
        services.AddSingleton(sp =>
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            return new BlobServiceClient(connectionString);
        });

        services.AddSingleton(sp =>
        {
            var endpoint = configuration["AzureDocumentIntelligence:Endpoint"];
            var apiKey = configuration["AzureDocumentIntelligence:ApiKey"];
            
            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException(
                    "Configuração do Azure Document Intelligence não encontrada. " +
                    "Configure 'AzureDocumentIntelligence:Endpoint' e 'AzureDocumentIntelligence:ApiKey'");
            }

            var credential = new Azure.AzureKeyCredential(apiKey);
            return new DocumentAnalysisClient(new Uri(endpoint), credential);
        });

        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IDocumentIntelligenceService, AzureDocumentIntelligenceService>();
        
        // Background Check Service (STUB - substituir por implementação real)
        services.AddScoped<IBackgroundCheckService, StubBackgroundCheckService>();

        // Background Workers
        services.AddHostedService<DocumentVerificationWorker>();

        return services;
    }
}
