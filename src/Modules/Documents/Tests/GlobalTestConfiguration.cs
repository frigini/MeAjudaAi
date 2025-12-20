using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Shared.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    /// <param name="services">Coleção de serviços</param>
    /// <param name="azuriteConnectionString">String de conexão para o Azurite blob storage (usada apenas quando useAzurite=true)</param>
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
            // Garante que BlobServiceClient e IBlobStorageService estão registrados para testes com Azurite
            // Trata casos onde o serviço pode não estar registrado ou foi condicionalmente ignorado
            if (!string.IsNullOrEmpty(azuriteConnectionString))
            {
                // Remove registros existentes de IBlobStorageService para evitar conflitos
                services.RemoveAll<IBlobStorageService>();

                // Remove registros existentes de BlobServiceClient
                services.RemoveAll<Azure.Storage.Blobs.BlobServiceClient>();

                // Registra BlobServiceClient com connection string do Azurite
                services.AddSingleton(sp =>
                    new Azure.Storage.Blobs.BlobServiceClient(azuriteConnectionString));

                // Registra AzureBlobStorageService real
                services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
            }
            else
            {
                // Fallback para mock se connection string não estiver disponível
                services.AddScoped<IBlobStorageService, MockBlobStorageService>();
            }
        }

        // DocumentIntelligenceService sempre usa mock (API externa cara)
        services.AddScoped<IDocumentIntelligenceService, MockDocumentIntelligenceService>();
        
        return services;
    }
}
