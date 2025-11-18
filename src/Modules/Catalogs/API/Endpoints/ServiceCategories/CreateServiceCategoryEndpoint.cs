using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.ServiceCategories;

public sealed record CreateServiceCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder = 0);

public class CreateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", CreateAsync)
            .WithName("CreateServiceCategory")
            .WithSummary("Criar nova categoria de serviço")
            .WithDescription("Cria uma nova categoria de serviço no catálogo admin-managed.")
            .Produces<Response<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("Admin");

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCategoryCommand(request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<CreateServiceCategoryCommand, Result<Guid>>(
            command, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        return Handle(result, "GetServiceCategoryById", new { id = result.Value });
    }
}
