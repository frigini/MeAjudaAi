using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace MeAjudaAi.Modules.Providers.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Providers.
/// </summary>
/// <remarks>
/// Utiliza o sistema unificado de versionamento via BaseEndpoint e organiza
/// todos os endpoints relacionados a prestadores de serviços em um grupo
/// versionado com autorização global aplicada.
/// </remarks>
public static class ProvidersModuleEndpoints
{
    /// <summary>
    /// Mapeia todos os endpoints do módulo Providers.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    /// <remarks>
    /// Configura um grupo versionado em "/api/v1/providers" com:
    /// - Autorização global obrigatória
    /// - Tag "Providers" para documentação OpenAPI
    /// - Todos os endpoints de administração de prestadores
    /// 
    /// **Endpoints incluídos:**
    /// - GET / - Listar prestadores (com paginação e filtros)
    /// - POST / - Criar prestador
    /// - GET /{id} - Buscar por ID
    /// - GET /by-user/{userId} - Buscar por ID do usuário
    /// - GET /by-city/{city} - Buscar por cidade
    /// - GET /by-state/{state} - Buscar por estado
    /// - GET /by-type/{type} - Buscar por tipo
    /// - GET /by-verification-status/{status} - Buscar por status
    /// - PUT /{id} - Atualizar perfil
    /// - POST /{id}/documents - Adicionar documento
    /// - DELETE /{id}/documents/{documentType} - Remover documento
    /// - PUT /{id}/verification-status - Atualizar status
    /// - DELETE /{id} - Excluir prestador
    /// </remarks>
    public static void MapProvidersEndpoints(this WebApplication app)
    {
        // Usa o sistema unificado de versionamento via BaseEndpoint
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, "providers", "Providers");
        // Remove .RequireAuthorization() global - cada endpoint define sua própria autorização

        // Endpoints de gestão de prestadores
        endpoints.MapEndpoint<GetProvidersEndpoint>()
            .MapEndpoint<CreateProviderEndpoint>()
            .MapEndpoint<GetProviderByIdEndpoint>()
            .MapEndpoint<GetProviderByUserIdEndpoint>()
            .MapEndpoint<GetProvidersByCityEndpoint>()
            .MapEndpoint<GetProvidersByStateEndpoint>()
            .MapEndpoint<GetProvidersByTypeEndpoint>()
            .MapEndpoint<GetProvidersByVerificationStatusEndpoint>()
            .MapEndpoint<UpdateProviderProfileEndpoint>()
            .MapEndpoint<AddDocumentEndpoint>()
            .MapEndpoint<RemoveDocumentEndpoint>()
            .MapEndpoint<UpdateVerificationStatusEndpoint>()
            .MapEndpoint<RequireBasicInfoCorrectionEndpoint>()
            .MapEndpoint<DeleteProviderEndpoint>();
    }
}
