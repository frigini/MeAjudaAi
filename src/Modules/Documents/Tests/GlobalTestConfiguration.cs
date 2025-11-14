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
    /// Registra serviços mock para testes unitários
    /// </summary>
    public static IServiceCollection AddDocumentsTestServices(this IServiceCollection services)
    {
        services.AddScoped<IBlobStorageService, MockBlobStorageService>();
        services.AddScoped<IDocumentIntelligenceService, MockDocumentIntelligenceService>();
        return services;
    }
}
