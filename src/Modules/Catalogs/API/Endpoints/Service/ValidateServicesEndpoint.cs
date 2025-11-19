using MeAjudaAi.Modules.Catalogs.Application.DTOs.Requests.Service;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Contracts.Modules.Catalogs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.Service;

public class ValidateServicesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/validate", ValidateAsync)
            .WithName("ValidateServices")
            .WithSummary("Validar múltiplos serviços")
            .Produces<Response<ValidateServicesResponse>>(StatusCodes.Status200OK)
            .RequireAdmin();

    private static async Task<IResult> ValidateAsync(
        [FromBody] ValidateServicesRequest request,
        [FromServices] ICatalogsModuleApi moduleApi,
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
