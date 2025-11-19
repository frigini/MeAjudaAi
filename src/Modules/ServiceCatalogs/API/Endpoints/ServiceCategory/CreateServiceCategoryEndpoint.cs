using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

public record CreateServiceCategoryRequest(string Name, string? Description, int DisplayOrder);

public class CreateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", CreateAsync)
            .WithName("CreateServiceCategory")
            .WithSummary("Criar categoria de serviço")
            .Produces<Response<ServiceCategoryDto>>(StatusCodes.Status201Created)
            .RequireAdmin();

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCategoryCommand(request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>(
            command, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        if (result.Value is null)
            return Results.BadRequest("Unexpected null value in successful result.");

        return Handle(result, "GetServiceCategoryById", new { id = result.Value.Id });
    }
}
