using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validator para UpdateProviderProfileRequest.
/// </summary>
public class UpdateProviderProfileRequestValidator : AbstractValidator<UpdateProviderProfileRequest>
{
    public UpdateProviderProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters long")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.BusinessProfile)
            .NotNull()
            .WithMessage("BusinessProfile is required");

        When(x => x.BusinessProfile != null, () =>
        {
            RuleFor(x => x.BusinessProfile!.Description)
                .NotEmpty()
                .WithMessage("BusinessProfile.Description is required")
                .MaximumLength(500)
                .WithMessage("BusinessProfile.Description cannot exceed 500 characters");

            RuleFor(x => x.BusinessProfile!.ContactInfo)
                .NotNull()
                .WithMessage("BusinessProfile.ContactInfo is required");

            When(x => x.BusinessProfile?.ContactInfo != null, () =>
            {
                RuleFor(x => x.BusinessProfile!.ContactInfo!.Email)
                    .NotEmpty()
                    .WithMessage("Email is required")
                    .EmailAddress()
                    .WithMessage("Email must be a valid email address");
            });

            RuleFor(x => x.BusinessProfile!.PrimaryAddress)
                .NotNull()
                .WithMessage("BusinessProfile.PrimaryAddress is required");

            When(x => x.BusinessProfile?.PrimaryAddress != null, () =>
            {
                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.Street)
                    .NotEmpty()
                    .WithMessage("PrimaryAddress.Street is required");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.City)
                    .NotEmpty()
                    .WithMessage("PrimaryAddress.City is required");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.State)
                    .NotEmpty()
                    .WithMessage("PrimaryAddress.State is required");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.ZipCode)
                    .NotEmpty()
                    .WithMessage("PrimaryAddress.ZipCode is required");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.Country)
                    .NotEmpty()
                    .WithMessage("PrimaryAddress.Country is required");
            });
        });
    }
}