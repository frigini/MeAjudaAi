using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para validar a existência e status de múltiplos serviços.
/// Requer privilégios de administrador.
/// </summary>
public class ValidateServicesEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint POST /validate para validar serviços em lote.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.ServiceCatalogs.Services.Validate, ValidateAsync)
            .WithName("ValidateServices")
            .WithSummary("Validar múltiplos serviços")
            .WithDescription("""
                Valida a existência e status de uma lista de serviços.
                
                **Funcionalidade:**
                - Verifica se todos os IDs existem no catálogo
                - Retorna quais serviços são válidos e quais são inválidos
                - Indica serviços inativos separadamente
                
                **Casos de Uso:**
                - Validar serviços antes de adicionar a um provedor
                - Verificação em lote para importação de dados
                - Garantir integridade referencial entre módulos
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces<Response<ValidateServicesResponse>>(StatusCodes.Status200OK)
            .RequireAdmin();

    private static async Task<IResult> ValidateAsync(
        [FromBody] ValidateServicesRequest request,
        [FromServices] IServiceCatalogsModuleApi moduleApi,
        CancellationToken cancellationToken)
    {
        var result = await moduleApi.ValidateServicesAsync(request.ServiceIds, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        // Mapear do DTO do módulo para o DTO da API
        var response = new ValidateServicesResponse(
            result.Value.AllValid,
            result.Value.InvalidServiceIds,
            result.Value.InactiveServiceIds
        );

        return Handle(Result<ValidateServicesResponse>.Success(response));
    }
}
