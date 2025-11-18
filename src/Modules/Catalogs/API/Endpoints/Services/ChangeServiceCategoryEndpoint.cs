using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.Services;

public class ChangeServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/change-category", ChangeAsync)
            .WithName("ChangeServiceCategory")
            .WithSummary("Alterar categoria do serviço")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> ChangeAsync(
        Guid id,
        [FromBody] ChangeServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeServiceCategoryCommand(id, request.NewCategoryId);
        var result = await commandDispatcher.SendAsync<ChangeServiceCategoryCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}
