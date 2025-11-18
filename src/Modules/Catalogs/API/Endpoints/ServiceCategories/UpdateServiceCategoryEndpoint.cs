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

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.ServiceCategories;

public class UpdateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateServiceCategory")
            .WithSummary("Atualizar categoria")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCategoryCommand(id, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<UpdateServiceCategoryCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}
