using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Users.DTOs;

/// <summary>
/// DTO para registro de novo cliente via API pública.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record RegisterCustomerRequestDto(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    bool TermsAccepted,
    bool AcceptedPrivacyPolicy
);
