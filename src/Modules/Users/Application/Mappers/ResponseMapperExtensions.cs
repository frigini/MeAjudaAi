using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs;

namespace MeAjudaAi.Modules.Users.Application.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs internos para DTOs de contrato do módulo Users.
/// </summary>
public static class ResponseMapperExtensions
{
    /// <summary>
    /// Mapeia UserDto (interno) para ModuleUserDto (contrato).
    /// </summary>
    public static ModuleUserDto ToContract(this UserDto user)
    {
        return new ModuleUserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.DeviceToken,
            user.PhoneNumber);
    }

    /// <summary>
    /// Mapeia UserDto (interno) para ModuleUserBasicDto (contrato básico).
    /// </summary>
    public static ModuleUserBasicDto ToBasicContract(this UserDto user)
    {
        return new ModuleUserBasicDto(
            user.Id,
            user.Username,
            user.Email,
            IsActive: user.IsActive);
    }

    /// <summary>
    /// Mapeia uma coleção de UserDto para ModuleUserBasicDto.
    /// </summary>
    public static IReadOnlyList<ModuleUserBasicDto> ToBasicContract(this IEnumerable<UserDto> users)
    {
        return users.Select(ToBasicContract).ToList();
    }
}
