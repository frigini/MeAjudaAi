using FluentValidation;
using MeAjudaAi.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Web.Admin.Extensions;

namespace MeAjudaAi.Web.Admin.Validators;

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
