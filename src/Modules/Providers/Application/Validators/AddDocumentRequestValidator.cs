using FluentValidation;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Modules.Providers.Application.Validators;

/// <summary>
/// Validator para AddDocumentRequest.
/// </summary>
public class AddDocumentRequestValidator : AbstractValidator<AddDocumentRequest>
{
    public AddDocumentRequestValidator()
    {
        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("Número do documento é obrigatório")
            .MinimumLength(3)
            .WithMessage("Número do documento deve ter pelo menos 3 caracteres")
            .MaximumLength(50)
            .WithMessage("Número do documento não pode exceder 50 caracteres")
            .Matches(@"^[a-zA-Z0-9\-\.]+$")
            .WithMessage("Número do documento deve conter apenas letras, números, hífens e pontos");

        RuleFor(x => x.DocumentType)
            .Must(BeValidDocumentType)
            .WithMessage("Tipo de documento inválido. Valores aceitos: None, CPF, CNPJ, RG, CNH, Passport, Other");
    }

    private static bool BeValidDocumentType(EDocumentType documentType)
    {
        return documentType.ToString().IsValidEnum<EDocumentType>();
    }
}
