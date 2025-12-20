using Azure.Storage;
using Azure.Storage.Blobs;
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
                services.RemoveAll<BlobServiceClient>();

                // Para Azurite, usa credenciais explícitas para suportar geração de SAS tokens
                // Connection string padrão do Azurite: "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=...;BlobEndpoint=http://..."
                // Credenciais padrão do Azurite (bem conhecidas, não são secretas)
                const string azuriteAccountName = "devstoreaccount1";
                const string azuriteAccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

                // Registra BlobServiceClient com credenciais explícitas para suportar SAS
                services.AddSingleton(sp =>
                {
                    var credential = new StorageSharedKeyCredential(azuriteAccountName, azuriteAccountKey);
                    
                    // Extrai o Blob Endpoint da connection string do Azurite
                    // Formato: "BlobEndpoint=http://127.0.0.1:xxxxx/devstoreaccount1"
                    var blobEndpointMatch = System.Text.RegularExpressions.Regex.Match(
                        azuriteConnectionString,
                        @"BlobEndpoint=(https?://[^;]+)");
                    
                    if (!blobEndpointMatch.Success)
                    {
                        throw new InvalidOperationException(
                            $"Could not extract BlobEndpoint from Azurite connection string: {azuriteConnectionString}");
                    }
                    
                    var blobEndpoint = new Uri(blobEndpointMatch.Groups[1].Value);
                    return new BlobServiceClient(blobEndpoint, credential);
                });

                // Registra AzureBlobStorageService real
                services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
            }
            else
            {
                // Fallback para mock se connection string não estiver disponível
                // AVISO: useAzurite=true foi solicitado mas sem connection string
                System.Diagnostics.Debug.WriteLine(
                    "WARNING: useAzurite=true requested but azuriteConnectionString is null/empty. Falling back to MockBlobStorageService.");
                services.AddScoped<IBlobStorageService, MockBlobStorageService>();
            }
        }

        // DocumentIntelligenceService sempre usa mock (API externa cara)
        services.AddScoped<IDocumentIntelligenceService, MockDocumentIntelligenceService>();
        
        return services;
    }
}
