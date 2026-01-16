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
    private static readonly string[] AllowedContentTypes = 
    { 
        "application/pdf", 
        "image/jpeg", 
        "image/jpg", 
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
            .Must(contentType => AllowedContentTypes.Contains(contentType.ToLowerInvariant()))
            .WithMessage($"Tipo de arquivo não permitido. Tipos aceitos: {string.Join(", ", AllowedContentTypes)}");
    }
}

/// <summary>
/// DTO para upload de documento com validação.
/// Usado para encapsular os dados do upload.
/// </summary>
public class UploadDocumentDto
{
    public Guid ProviderId { get; set; }
    public IBrowserFile? File { get; set; }
    public string DocumentType { get; set; } = string.Empty;
}

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

    private static readonly string[] ValidDocumentTypes =
    {
        "RG",
        "CNH",
        "CPF",
        "CNPJ",
        "ComprovanteResidencia",
        "CertidaoNascimento",
        "Outros"
    };

    private static bool IsValidDocumentType(string type) => ValidDocumentTypes.Contains(type);
}
