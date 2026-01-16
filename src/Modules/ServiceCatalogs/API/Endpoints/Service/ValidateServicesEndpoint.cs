using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using MeAjudaAi.Shared.Models;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class ValidateServicesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("validate", ValidateAsync)
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
            result.Value!.AllValid,
            result.Value.InvalidServiceIds,
            result.Value.InactiveServiceIds
        );

        return Handle(Result<ValidateServicesResponse>.Success(response));
    }
}
