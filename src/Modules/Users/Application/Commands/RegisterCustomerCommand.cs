using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Users.Application.Commands;

public sealed record RegisterCustomerCommand(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    bool TermsAccepted,
    bool AcceptedPrivacyPolicy
) : Command<Result<UserDto>>;
