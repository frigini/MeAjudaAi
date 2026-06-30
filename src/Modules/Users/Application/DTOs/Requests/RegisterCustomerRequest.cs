using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public record RegisterCustomerRequest(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    bool TermsAccepted,
    bool AcceptedPrivacyPolicy
);
