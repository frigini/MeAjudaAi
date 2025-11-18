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

public class CreateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", CreateAsync)
            .WithName("CreateService")
            .WithSummary("Criar novo serviço")
            .Produces<Response<Guid>>(StatusCodes.Status201Created)
            .RequireAuthorization("Admin");

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateServiceRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCommand(request.CategoryId, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<CreateServiceCommand, Result<Guid>>(command, cancellationToken);
        
        if (!result.IsSuccess)
            return Handle(result);
        
        return Handle(result, "GetServiceById", new { id = result.Value });
    }
}
