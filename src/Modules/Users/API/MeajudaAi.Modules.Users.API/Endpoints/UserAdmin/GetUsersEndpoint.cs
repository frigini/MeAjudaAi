using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

public class GetUsersEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/api/v1/users", GetUsersAsync)
            .WithName("GetUsers")
            .WithSummary("Get paginated users")
            .WithDescription("Retrieves a paginated list of users")
            .RequireAuthorization("UserManagement")
            .Produces<PagedResponse<IEnumerable<UserDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> GetUsersAsync(
        [AsParameters] GetUsersRequest request,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm);

        var result = await queryDispatcher.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            query, cancellationToken);

        return HandlePagedResult(result);
    }
}