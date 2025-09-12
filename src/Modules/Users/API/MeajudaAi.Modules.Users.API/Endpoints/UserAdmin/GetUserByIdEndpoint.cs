using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

public class GetUserByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}", GetUserAsync)
            .WithName("GetUser")
            .WithSummary("Get user by ID")
            .WithDescription("Retrieves a specific user by their unique identifier")
            .RequireAuthorization("SelfOrAdmin")
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetUserAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await queryDispatcher.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
            query, cancellationToken);

        return Handle(result);
    }
}