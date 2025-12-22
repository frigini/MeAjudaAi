using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Queries;

namespace MeAjudaAi.Modules.Users.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands e Queries
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia CreateUserRequest para CreateUserCommand
    /// </summary>
    /// <param name="request">Requisição de criação de usuário</param>
    /// <returns>CreateUserCommand com propriedades mapeadas</returns>
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
    /// Mapeia UpdateUserProfileRequest para UpdateUserProfileCommand
    /// </summary>
    /// <param name="request">Requisição de atualização de perfil</param>
    /// <param name="userId">ID do usuário a ser atualizado</param>
    /// <returns>UpdateUserProfileCommand com propriedades mapeadas</returns>
    public static UpdateUserProfileCommand ToCommand(this UpdateUserProfileRequest request, Guid userId)
    {
        return new UpdateUserProfileCommand(
            UserId: userId,
            FirstName: request.FirstName,
            LastName: request.LastName,
            Email: request.Email,
            PhoneNumber: request.PhoneNumber
        );
    }

    /// <summary>
    /// Mapeia o ID do usuário para DeleteUserCommand
    /// </summary>
    /// <param name="userId">ID do usuário a ser excluído</param>
    /// <returns>DeleteUserCommand com o ID especificado</returns>
    public static DeleteUserCommand ToDeleteCommand(this Guid userId)
    {
        return new DeleteUserCommand(userId);
    }

    /// <summary>
    /// Mapeia o ID do usuário para GetUserByIdQuery
    /// </summary>
    /// <param name="userId">ID do usuário a ser consultado</param>
    /// <returns>GetUserByIdQuery com o ID especificado</returns>
    public static GetUserByIdQuery ToQuery(this Guid userId)
    {
        return new GetUserByIdQuery(userId);
    }

    /// <summary>
    /// Mapeia o email para GetUserByEmailQuery
    /// </summary>
    /// <param name="email">Email do usuário a ser consultado</param>
    /// <returns>GetUserByEmailQuery com o email especificado</returns>
    public static GetUserByEmailQuery ToEmailQuery(this string? email)
    {
        return new GetUserByEmailQuery(email ?? string.Empty);
    }

    /// <summary>
    /// Mapeia GetUsersRequest para GetUsersQuery
    /// </summary>
    /// <param name="request">Requisição de listagem de usuários</param>
    /// <returns>GetUsersQuery com os parâmetros especificados</returns>
    public static GetUsersQuery ToUsersQuery(this GetUsersRequest request)
    {
        return new GetUsersQuery(
            Page: request.PageNumber,
            PageSize: request.PageSize,
            SearchTerm: request.SearchTerm
        );
    }
}
