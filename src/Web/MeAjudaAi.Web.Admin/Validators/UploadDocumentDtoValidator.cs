using FluentValidation;
using MeAjudaAi.Web.Admin.DTOs;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para UploadDocumentDto.
/// </summary>
public class UploadDocumentDtoValidator : AbstractValidator<UploadDocumentDto>
{
    public UploadDocumentDtoValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty()
            .WithMessage("Provider deve ser selecionado");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Arquivo é obrigatório")
            .SetValidator(new UploadDocumentValidator()!);

        RuleFor(x => x.DocumentType)
            .NotEmpty()
            .WithMessage("Tipo de documento é obrigatório")
            .Must(type => IsValidDocumentType(type))
            .WithMessage("Tipo de documento inválido");
    }

    private static readonly HashSet<string> ValidDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "RG",
        "CNH",
        "CPF",
        "CNPJ",
        "ComprovanteResidencia",
        "CertidaoNascimento",
        "Outros"
    };

    private static bool IsValidDocumentType(string type) => ValidDocumentTypes.Contains(type.Trim());
}
