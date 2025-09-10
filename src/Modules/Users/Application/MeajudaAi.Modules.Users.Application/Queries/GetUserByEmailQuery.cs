using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Queries;

public sealed record GetUserByEmailQuery(string Email) : Query<Result<UserDto>>;