using FluentValidation;
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para CreateProviderRequestDto com regras de negócio do Brasil.
/// </summary>
public class CreateProviderRequestDtoValidator : AbstractValidator<CreateProviderRequestDto>
{
    public CreateProviderRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MinimumLength(3)
            .WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(200)
            .WithMessage("Nome deve ter no máximo 200 caracteres")
            .NoXss()
            .WithMessage("Nome contém caracteres não permitidos");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Tipo de provider inválido");

        // Validação de documento (CPF/CNPJ) - opcional mas deve ser válido se informado
        When(x => !string.IsNullOrWhiteSpace(x.Document), () =>
        {
            RuleFor(x => x.Document)
                .ValidCpfOrCnpj()
                .WithMessage("Documento inválido. Informe um CPF ou CNPJ válido");
        });

        // Validações do BusinessProfile
        RuleFor(x => x.BusinessProfile)
            .NotNull()
            .WithMessage("Perfil de negócio é obrigatório")
            .SetValidator(new BusinessProfileDtoValidator());
    }
}

/// <summary>
/// Validador para BusinessProfileDto.
/// </summary>
public class BusinessProfileDtoValidator : AbstractValidator<BusinessProfileDto>
{
    public BusinessProfileDtoValidator()
    {
        RuleFor(x => x.LegalName)
            .NotEmpty()
            .WithMessage("Razão social é obrigatória")
            .MinimumLength(3)
            .WithMessage("Razão social deve ter no mínimo 3 caracteres")
            .MaximumLength(200)
            .WithMessage("Razão social deve ter no máximo 200 caracteres")
            .NoXss();

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

        RuleFor(x => x.ContactInfo)
            .NotNull()
            .WithMessage("Informações de contato são obrigatórias")
            .SetValidator(new ContactInfoDtoValidator());

        RuleFor(x => x.PrimaryAddress)
            .NotNull()
            .WithMessage("Endereço é obrigatório")
            .SetValidator(new PrimaryAddressDtoValidator());
    }
}

/// <summary>
/// Validador para ContactInfoDto.
/// </summary>
public class ContactInfoDtoValidator : AbstractValidator<ContactInfoDto>
{
    public ContactInfoDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email é obrigatório")
            .ValidEmail()
            .MaximumLength(100)
            .WithMessage("Email deve ter no máximo 100 caracteres");

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
/// Validador para PrimaryAddressDto.
/// </summary>
public class PrimaryAddressDtoValidator : AbstractValidator<PrimaryAddressDto>
{
    public PrimaryAddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Rua é obrigatória")
            .MinimumLength(3)
            .WithMessage("Rua deve ter no mínimo 3 caracteres")
            .MaximumLength(200)
            .WithMessage("Rua deve ter no máximo 200 caracteres")
            .NoXss();

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

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("Cidade é obrigatória")
            .MinimumLength(2)
            .WithMessage("Cidade deve ter no mínimo 2 caracteres")
            .MaximumLength(100)
            .WithMessage("Cidade deve ter no máximo 100 caracteres")
            .NoXss();

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("Estado é obrigatório")
            .Length(2)
            .WithMessage("Estado deve ter 2 caracteres (UF)")
            .Matches(@"^[A-Z]{2}$")
            .WithMessage("Estado deve ser uma UF válida (ex: SP, RJ)");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("CEP é obrigatório")
            .ValidCep();

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("País é obrigatório")
            .NoXss()
            .MinimumLength(2)
            .WithMessage("País deve ter no mínimo 2 caracteres")
            .MaximumLength(100)
            .WithMessage("País deve ter no máximo 100 caracteres");
    }
}
