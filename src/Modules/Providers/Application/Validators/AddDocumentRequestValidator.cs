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
            .WithMessage("Document number is required")
            .MinimumLength(3)
            .WithMessage("Document number must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Document number cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9\-\.]+$")
            .WithMessage("Document number can only contain letters, numbers, hyphens and dots");

        RuleFor(x => x.DocumentType)
            .Must(BeValidDocumentType)
            .WithMessage($"DocumentType must be a valid document type. {EnumExtensions.GetValidValuesDescription<EDocumentType>()}");
    }

    private static bool BeValidDocumentType(EDocumentType documentType)
    {
        return documentType.ToString().IsValidEnum<EDocumentType>();
    }
}