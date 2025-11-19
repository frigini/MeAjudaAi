using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

public record UpdateServiceCategoryRequest(string Name, string? Description, int DisplayOrder);

public class UpdateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateServiceCategory")
            .WithSummary("Atualizar categoria de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCategoryCommand(id, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<UpdateServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
