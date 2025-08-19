using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
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
            .Produces<PagedResponse<IEnumerable<UserDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> GetUsersAsync(
        [AsParameters] GetUsersRequest request,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetUsersAsync(request, cancellationToken);
        if (result.IsFailure)
            return Ok(result);

        var totalCountResult = await userService.GetTotalUsersCountAsync(cancellationToken);
        if (totalCountResult.IsFailure)
            return Ok(totalCountResult);

        return Paged(result, totalCountResult.Value, request.PageNumber, request.PageSize);
    }
}