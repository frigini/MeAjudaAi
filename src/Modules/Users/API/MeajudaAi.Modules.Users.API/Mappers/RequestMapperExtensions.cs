using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Queries;

namespace MeAjudaAi.Modules.Users.API.Mappers;

/// <summary>
/// Extension methods for mapping DTOs to Commands and Queries
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Maps CreateUserRequest to CreateUserCommand
    /// </summary>
    /// <param name="request">The user creation request</param>
    /// <returns>CreateUserCommand with mapped properties</returns>
    public static CreateUserCommand ToCommand(this CreateUserRequest request)
    {
        return new CreateUserCommand(
            Username: request.Username,
            Email: request.Email,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Password: request.Password,
            Roles: request.Roles ?? Array.Empty<string>()
        );
    }

    /// <summary>
    /// Maps UpdateUserProfileRequest to UpdateUserProfileCommand
    /// </summary>
    /// <param name="request">The profile update request</param>
    /// <param name="userId">The ID of the user to update</param>
    /// <returns>UpdateUserProfileCommand with mapped properties</returns>
    public static UpdateUserProfileCommand ToCommand(this UpdateUserProfileRequest request, Guid userId)
    {
        return new UpdateUserProfileCommand(
            UserId: userId,
            FirstName: request.FirstName,
            LastName: request.LastName
            // Note: Email is not included as per command design - use separate command for email updates
        );
    }

    /// <summary>
    /// Maps user ID to DeleteUserCommand
    /// </summary>
    /// <param name="userId">The ID of the user to delete</param>
    /// <returns>DeleteUserCommand with the specified user ID</returns>
    public static DeleteUserCommand ToDeleteCommand(this Guid userId)
    {
        return new DeleteUserCommand(userId);
    }

    /// <summary>
    /// Maps user ID to GetUserByIdQuery
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve</param>
    /// <returns>GetUserByIdQuery with the specified user ID</returns>
    public static GetUserByIdQuery ToQuery(this Guid userId)
    {
        return new GetUserByIdQuery(userId);
    }

    /// <summary>
    /// Maps email to GetUserByEmailQuery
    /// </summary>
    /// <param name="email">The email of the user to retrieve</param>
    /// <returns>GetUserByEmailQuery with the specified email</returns>
    public static GetUserByEmailQuery ToEmailQuery(this string? email)
    {
        return new GetUserByEmailQuery(email ?? string.Empty);
    }

    /// <summary>
    /// Maps GetUsersRequest to GetUsersQuery
    /// </summary>
    /// <param name="request">The users listing request</param>
    /// <returns>GetUsersQuery with the specified parameters</returns>
    public static GetUsersQuery ToUsersQuery(this GetUsersRequest request)
    {
        return new GetUsersQuery(
            Page: request.PageNumber,
            PageSize: request.PageSize,
            SearchTerm: request.SearchTerm
        );
    }


}