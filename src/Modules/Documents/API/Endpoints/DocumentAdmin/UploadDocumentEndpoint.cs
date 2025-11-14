using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.DocumentAdmin;

/// <summary>
/// Endpoint responsável pela geração de URL de upload com SAS token.
/// </summary>
public class UploadDocumentEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de upload de documento.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/upload", UploadDocumentAsync)
            .WithName("UploadDocument")
            .WithSummary("Gerar URL de upload com SAS token")
            .WithDescription("""
                Gera uma URL de upload com SAS token para envio direto ao Azure Blob Storage.
                
                **Fluxo:**
                1. Valida informações do documento
                2. Gera SAS token com 1 hora de validade
                3. Cria registro de documento no banco com status Uploaded
                4. Retorna URL de upload (com blob name e data de expiração)
                
                **Tipos de documentos suportados:**
                - IdentityDocument: RG, CNH, CPF
                - ProofOfResidence: Comprovante de residência
                - CriminalRecord: Certidão de antecedentes
                - Other: Outros documentos
                """)
            .Produces<UploadDocumentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> UploadDocumentAsync(
        [FromBody] UploadDocumentRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BaseEndpoint.BadRequest("Request body is required");

        var command = new UploadDocumentCommand(
            request.ProviderId,
            request.DocumentType.ToString(),
            request.FileName,
            request.ContentType,
            request.FileSizeBytes);

        var response = await commandDispatcher.SendAsync<UploadDocumentCommand, UploadDocumentResponse>(
            command, cancellationToken);

        return Results.Ok(response);
    }
}
