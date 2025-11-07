using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validator para UpdateVerificationStatusRequest.
/// </summary>
public class UpdateVerificationStatusRequestValidator : AbstractValidator<UpdateVerificationStatusRequest>
{
    public UpdateVerificationStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidVerificationStatus)
            .WithMessage($"Status must be a valid verification status. {EnumExtensions.GetValidValuesDescription<EVerificationStatus>()}");

        When(x => !string.IsNullOrWhiteSpace(x.Notes), () =>
        {
            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .WithMessage("Notes cannot exceed 1000 characters");
        });
    }

    private static bool BeValidVerificationStatus(EVerificationStatus status)
    {
        return status.ToString().IsValidEnum<EVerificationStatus>();
    }
}