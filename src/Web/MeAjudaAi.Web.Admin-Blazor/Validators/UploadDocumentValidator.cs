using FluentValidation;
using MeAjudaAi.Web.Admin.Extensions;
using Microsoft.AspNetCore.Components.Forms;

namespace MeAjudaAi.Web.Admin.Validators;

/// <summary>
/// Validador para upload de documentos (IBrowserFile).
/// Valida tipo de arquivo, tamanho e segurança.
/// </summary>
public class UploadDocumentValidator : AbstractValidator<IBrowserFile>
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    public UploadDocumentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nome do arquivo é obrigatório")
            .ValidFileType(AllowedExtensions)
            .NoXss()
            .WithMessage("Nome do arquivo contém caracteres não permitidos");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .WithMessage("Arquivo vazio não é permitido")
            .MaxFileSize(MaxFileSize);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Tipo de conteúdo é obrigatório")
            .Must(contentType => AllowedContentTypes.Contains(contentType.Trim()))
            .WithMessage($"Tipo de arquivo não permitido. Tipos aceitos: {string.Join(", ", AllowedContentTypes)}");
    }
}
