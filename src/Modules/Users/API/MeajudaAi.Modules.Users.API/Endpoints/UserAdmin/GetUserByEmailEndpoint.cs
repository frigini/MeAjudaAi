using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

public class GetUserByEmailEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/by-email/{email}", GetUserByEmailAsync)
            .WithName("GetUserByEmail")
            .WithSummary("Get user by email")
            .WithDescription("Retrieves a specific user by their email address")
            .RequireAuthorization("AdminOnly")
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetUserByEmailAsync(
        string email,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await queryDispatcher.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            query, cancellationToken);

        return Handle(result);
    }
}