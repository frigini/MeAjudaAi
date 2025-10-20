using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Domain.Entities;

namespace MeAjudaAi.Modules.Users.Application.Mappers;

public static class UserMappers
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id.Value,
            user.Username.Value,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.GetFullName(),
            user.KeycloakId,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
