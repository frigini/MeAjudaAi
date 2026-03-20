using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validator para CreateProviderRequest.
/// </summary>
public class CreateProviderRequestValidator : AbstractValidator<CreateProviderRequest>
{
    public CreateProviderRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters long")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Type)
            .Must(BeValidProviderType)
            .WithMessage($"Type must be a valid provider type. {EnumExtensions.GetValidValuesDescription<EProviderType>()}");

        RuleFor(x => x.BusinessProfile)
            .NotNull()
            .WithMessage("BusinessProfile is required");

        When(x => x.BusinessProfile != null, () =>
        {
            RuleFor(x => x.BusinessProfile!.Description)
                .MaximumLength(500)
                .WithMessage("BusinessProfile.Description cannot exceed 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.BusinessProfile?.Description));

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
                .WithMessage("Endereço principal é obrigatório")
                .When(x => x.BusinessProfile?.ShowAddressToClient == true);

            When(x => x.BusinessProfile?.PrimaryAddress != null, () =>
            {
                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.Street)
                    .NotEmpty()
                    .WithMessage("Rua é obrigatória");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.City)
                    .NotEmpty()
                    .WithMessage("Cidade é obrigatória");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.State)
                    .NotEmpty()
                    .WithMessage("Estado é obrigatório");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.ZipCode)
                    .NotEmpty()
                    .WithMessage("CEP é obrigatório");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.Country)
                    .NotEmpty()
                    .WithMessage("País é obrigatório");
            });
        });
    }



    private static bool BeValidProviderType(EProviderType type)
    {
        return type.ToString().IsValidEnum<EProviderType>();
    }
}
