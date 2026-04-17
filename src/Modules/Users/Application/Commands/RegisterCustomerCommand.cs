using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.Commands;

[ExcludeFromCodeCoverage]

public sealed record RegisterCustomerCommand(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    bool TermsAccepted,
    bool AcceptedPrivacyPolicy
) : Command<Result<UserDto>>;
