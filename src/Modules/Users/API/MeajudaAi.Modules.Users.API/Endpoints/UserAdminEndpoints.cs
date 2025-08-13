using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints;

public class UserAdminEndpoints : BaseEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = CreateGroup(app, "users", "User Management")
            .RequireAuthorization("AdminPolicy");

        group.MapGet("/", GetUsersAsync)
            .WithName("GetUsers")
            .WithSummary("Get paginated users")
            .Produces<PagedResponse<IEnumerable<UserDto>>>(200);

        group.MapGet("/{id:guid}", GetUserAsync)
            .WithName("GetUser")
            .WithSummary("Get user by ID")
            .Produces<Response<UserDto>>(200)
            .Produces<Response<object>>(404);

        group.MapPost("/", CreateUserAsync)
            .WithName("CreateUser")
            .WithSummary("Create new user")
            .Produces<Response<UserDto>>(201)
            .Produces<Response<object>>(400);

        group.MapPut("/{id:guid}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithSummary("Update user")
            .Produces<Response<UserDto>>(200)
            .Produces<Response<object>>(404);

        group.MapDelete("/{id:guid}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Delete user")
            .Produces<Response<object>>(204)
            .Produces<Response<object>>(404);
    }

    private static async Task<IResult> GetUsersAsync(
        [AsParameters] GetUsersRequest request,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetUsersAsync(request, cancellationToken);
        var totalCount = await userService.GetTotalUsersCountAsync(cancellationToken);

        return Paged(result, totalCount.Value, request.PageNumber, request.PageSize);
    }

    private static async Task<IResult> GetUserAsync(
        Guid id,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.CreateUserAsync(request, cancellationToken);
        return Created(result, "GetUser", new { id = result.Value?.Id });
    }

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(result);
    }

    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.DeleteUserAsync(id, cancellationToken);
        return NoContent(result);
    }
}