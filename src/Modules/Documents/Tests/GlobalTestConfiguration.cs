using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Shared.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Documents.Tests;

/// <summary>
/// Collection definition específica para testes de integração do módulo Documents
/// </summary>
[CollectionDefinition("DocumentsIntegrationTests")]
public class DocumentsIntegrationTestCollection : ICollectionFixture<SharedIntegrationTestFixture>
{
    // Esta classe não tem implementação - apenas define a collection específica do módulo Documents
}

/// <summary>
/// Configuração global de mocks para testes do módulo Documents
/// </summary>
public static class TestServicesConfiguration
{
    /// <summary>
    /// Registra serviços para testes
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="azuriteConnectionString">Connection string for Azurite blob storage (only used when useAzurite=true)</param>
    /// <param name="useAzurite">Se true, usa AzureBlobStorageService real (requer Azurite container). Se false, usa MockBlobStorageService</param>
    public static IServiceCollection AddDocumentsTestServices(
        this IServiceCollection services, 
        string? azuriteConnectionString = null,
        bool useAzurite = false)
    {
        // BlobStorageService: usa Azurite real ou Mock dependendo do parâmetro
        if (!useAzurite)
        {
            services.AddScoped<IBlobStorageService, MockBlobStorageService>();
        }
        else
        {
            // Ensure BlobServiceClient and IBlobStorageService are registered for Azurite tests
            // This handles cases where the service might not be registered yet or was conditionally skipped
            var storageConnectionString = azuriteConnectionString;
            
            if (!string.IsNullOrEmpty(storageConnectionString))
            {
                // Remove any existing IBlobStorageService registrations to avoid conflicts
                var existingDescriptors = services.Where(d => d.ServiceType == typeof(IBlobStorageService)).ToList();
                foreach (var descriptor in existingDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Remove any existing BlobServiceClient registrations
                var existingBlobClientDescriptors = services.Where(d => d.ServiceType == typeof(Azure.Storage.Blobs.BlobServiceClient)).ToList();
                foreach (var descriptor in existingBlobClientDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Register BlobServiceClient with Azurite connection string
                services.AddSingleton(sp =>
                    new Azure.Storage.Blobs.BlobServiceClient(storageConnectionString));

                // Register real AzureBlobStorageService
                services.AddScoped<IBlobStorageService, MeAjudaAi.Modules.Documents.Infrastructure.Services.AzureBlobStorageService>();
            }
            else
            {
                // Fallback to mock if connection string is not available
                services.AddScoped<IBlobStorageService, MockBlobStorageService>();
            }
        }

        // DocumentIntelligenceService sempre usa mock (API externa cara)
        services.AddScoped<IDocumentIntelligenceService, MockDocumentIntelligenceService>();
        
        return services;
    }
}
