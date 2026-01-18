using Microsoft.AspNetCore.Components.Forms;

namespace MeAjudaAi.Web.Admin.DTOs;

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
