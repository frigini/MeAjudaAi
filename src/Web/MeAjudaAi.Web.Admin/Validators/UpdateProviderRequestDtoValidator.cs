using FluentValidation;
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para UpdateProviderRequestDto com regras de negócio do Brasil.
/// Como todos os campos são opcionais, valida apenas quando informados.
/// </summary>
public class UpdateProviderRequestDtoValidator : AbstractValidator<UpdateProviderRequestDto>
{
    public UpdateProviderRequestDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(3)
                .WithMessage("Nome deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Nome deve ter no máximo 200 caracteres")
                .NoXss()
                .WithMessage("Nome contém caracteres não permitidos");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .ValidBrazilianPhone()
                .WithMessage("Telefone inválido. Use formato brasileiro: (00) 00000-0000");
        });

        When(x => x.BusinessProfile != null, () =>
        {
            RuleFor(x => x.BusinessProfile)
                .SetValidator(new BusinessProfileUpdateDtoValidator());
        });
    }
}

/// <summary>
/// Validador para BusinessProfileUpdateDto.
/// </summary>
public class BusinessProfileUpdateDtoValidator : AbstractValidator<BusinessProfileUpdateDto>
{
    public BusinessProfileUpdateDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.LegalName), () =>
        {
            RuleFor(x => x.LegalName)
                .MinimumLength(3)
                .WithMessage("Razão social deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Razão social deve ter no máximo 200 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.FantasyName), () =>
        {
            RuleFor(x => x.FantasyName)
                .MinimumLength(3)
                .WithMessage("Nome fantasia deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Nome fantasia deve ter no máximo 200 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .WithMessage("Descrição deve ter no máximo 1000 caracteres")
                .NoXss();
        });

        When(x => x.ContactInfo != null, () =>
        {
            RuleFor(x => x.ContactInfo)
                .SetValidator(new ContactInfoUpdateDtoValidator());
        });

        When(x => x.PrimaryAddress != null, () =>
        {
            RuleFor(x => x.PrimaryAddress)
                .SetValidator(new PrimaryAddressUpdateDtoValidator());
        });
    }
}

/// <summary>
/// Validador para ContactInfoUpdateDto.
/// </summary>
public class ContactInfoUpdateDtoValidator : AbstractValidator<ContactInfoUpdateDto>
{
    public ContactInfoUpdateDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .ValidEmail()
                .MaximumLength(100)
                .WithMessage("Email deve ter no máximo 100 caracteres");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .ValidBrazilianPhone()
                .WithMessage("Telefone inválido. Use formato brasileiro: (00) 00000-0000");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Website), () =>
        {
            RuleFor(x => x.Website)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                             (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                .WithMessage("Website deve ser uma URL válida (http ou https)")
                .MaximumLength(200)
                .WithMessage("Website deve ter no máximo 200 caracteres")
                .NoXss();
        });
    }
}

/// <summary>
/// Validador para PrimaryAddressUpdateDto.
/// </summary>
public class PrimaryAddressUpdateDtoValidator : AbstractValidator<PrimaryAddressUpdateDto>
{
    public PrimaryAddressUpdateDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Street), () =>
        {
            RuleFor(x => x.Street)
                .MinimumLength(3)
                .WithMessage("Rua deve ter no mínimo 3 caracteres")
                .MaximumLength(200)
                .WithMessage("Rua deve ter no máximo 200 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Number), () =>
        {
            RuleFor(x => x.Number)
                .MaximumLength(20)
                .WithMessage("Número deve ter no máximo 20 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Complement), () =>
        {
            RuleFor(x => x.Complement)
                .MaximumLength(100)
                .WithMessage("Complemento deve ter no máximo 100 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Neighborhood), () =>
        {
            RuleFor(x => x.Neighborhood)
                .MaximumLength(100)
                .WithMessage("Bairro deve ter no máximo 100 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.City), () =>
        {
            RuleFor(x => x.City)
                .MinimumLength(2)
                .WithMessage("Cidade deve ter no mínimo 2 caracteres")
                .MaximumLength(100)
                .WithMessage("Cidade deve ter no máximo 100 caracteres")
                .NoXss();
        });

        When(x => !string.IsNullOrWhiteSpace(x.State), () =>
        {
            RuleFor(x => x.State)
                .Length(2)
                .WithMessage("Estado deve ter 2 caracteres (UF)")
                .Matches(@"^[A-Z]{2}$")
                .WithMessage("Estado deve ser uma UF válida (ex: SP, RJ)");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ZipCode), () =>
        {
            RuleFor(x => x.ZipCode)
                .ValidCep();
        });

        When(x => !string.IsNullOrWhiteSpace(x.Country), () =>
        {
            RuleFor(x => x.Country)
                .NoXss()
                .MinimumLength(2)
                .WithMessage("País deve ter no mínimo 2 caracteres")
                .MaximumLength(100)
                .WithMessage("País deve ter no máximo 100 caracteres");
        });
    }
}
