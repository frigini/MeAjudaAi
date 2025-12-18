using MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace MeAjudaAi.Modules.Locations.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Locations.
/// </summary>
/// <remarks>
/// Utiliza o sistema unificado de versionamento via BaseEndpoint e organiza
/// todos os endpoints relacionados a cidades permitidas (Allowed Cities) em um grupo
/// versionado com autorização global aplicada.
/// </remarks>
public static class LocationsModuleEndpoints
{
    /// <summary>
    /// Mapeia todos os endpoints do módulo Locations.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    /// <remarks>
    /// Configura um grupo versionado em "/api/v1/admin/allowed-cities" com:
    /// - Autorização de Admin obrigatória (RequireAdmin)
    /// - Tag "Allowed Cities" para documentação OpenAPI
    /// - Todos os endpoints de administração de cidades permitidas
    /// 
    /// **Endpoints incluídos:**
    /// - POST /api/v1/admin/allowed-cities - Criar cidade permitida
    /// - GET /api/v1/admin/allowed-cities - Listar todas as cidades permitidas
    /// - GET /api/v1/admin/allowed-cities/{id} - Buscar cidade permitida por ID
    /// - PUT /api/v1/admin/allowed-cities/{id} - Atualizar cidade permitida
    /// - DELETE /api/v1/admin/allowed-cities/{id} - Excluir cidade permitida
    /// </remarks>
    public static void MapLocationsEndpoints(this WebApplication app)
    {
        // Usa o sistema unificado de versionamento via BaseEndpoint
        // Nota: Cada endpoint individual já aplica .RequireAdmin() conforme necessário
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, "admin/allowed-cities", "Allowed Cities");

        // Endpoints de gestão de cidades permitidas (Admin only)
        endpoints.MapEndpoint<CreateAllowedCityEndpoint>()
            .MapEndpoint<GetAllAllowedCitiesEndpoint>()
            .MapEndpoint<GetAllowedCityByIdEndpoint>()
            .MapEndpoint<UpdateAllowedCityEndpoint>()
            .MapEndpoint<DeleteAllowedCityEndpoint>();
    }
}
