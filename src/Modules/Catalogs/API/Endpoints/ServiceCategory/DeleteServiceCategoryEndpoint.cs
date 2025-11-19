using MeAjudaAi.Modules.Catalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.ServiceCategory;

public class DeleteServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteServiceCategory")
            .WithSummary("Deletar categoria de serviço")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeleteServiceCategoryCommand(id);
        var result = await commandDispatcher.SendAsync<DeleteServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}
