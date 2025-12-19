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
    /// <param name="useAzurite">Se true, usa AzureBlobStorageService real (requer Azurite container). Se false, usa MockBlobStorageService</param>
    public static IServiceCollection AddDocumentsTestServices(this IServiceCollection services, bool useAzurite = false)
    {
        // BlobStorageService: usa Azurite real ou Mock dependendo do parâmetro
        if (!useAzurite)
        {
            services.AddScoped<IBlobStorageService, MockBlobStorageService>();
        }
        // Se useAzurite=true, o serviço real já está registrado via AddDocumentsModule com Azure:Storage:ConnectionString configurado

        // DocumentIntelligenceService sempre usa mock (API externa cara)
        services.AddScoped<IDocumentIntelligenceService, MockDocumentIntelligenceService>();
        
        return services;
    }
}
