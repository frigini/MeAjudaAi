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
            .WithMessage("Nome é obrigatório")
            .MinimumLength(2)
            .WithMessage("Nome deve ter no mínimo 2 caracteres")
            .MaximumLength(100)
            .WithMessage("Nome não pode exceder 100 caracteres");

        RuleFor(x => x.BusinessProfile)
            .NotNull()
            .WithMessage("Perfil de negócio é obrigatório");

        When(x => x.BusinessProfile != null, () =>
        {
            RuleFor(x => x.BusinessProfile!.Description)
                .MaximumLength(500)
                .WithMessage("Descrição não pode exceder 500 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.BusinessProfile?.Description));

            RuleFor(x => x.BusinessProfile!.ContactInfo)
                .NotNull()
                .WithMessage("Informações de contato são obrigatórias");

            When(x => x.BusinessProfile?.ContactInfo != null, () =>
            {
                RuleFor(x => x.BusinessProfile!.ContactInfo!.Email)
                    .NotEmpty()
                    .WithMessage("Email é obrigatório")
                    .EmailAddress()
                    .WithMessage("Email deve ser um endereço de email válido");
            });

            RuleFor(x => x.BusinessProfile!.PrimaryAddress)
                .NotNull()
                .WithMessage("Endereço principal é obrigatório");

            When(x => x.BusinessProfile?.PrimaryAddress != null, () =>
            {
                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.Street)
                    .NotEmpty()
                    .WithMessage("Rua é obrigatória");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.Number)
                    .NotEmpty()
                    .WithMessage("Número é obrigatório");

                RuleFor(x => x.BusinessProfile!.PrimaryAddress!.Neighborhood)
                    .NotEmpty()
                    .WithMessage("Bairro é obrigatório");

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
}
