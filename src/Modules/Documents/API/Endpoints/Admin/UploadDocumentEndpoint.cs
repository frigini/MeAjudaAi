using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

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
            .Produces<Result<Contracts.Modules.Documents.DTOs.UploadDocumentResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> UploadDocumentAsync(
        [FromBody] Contracts.Modules.Documents.DTOs.UploadDocumentRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest("Corpo da requisição é obrigatório");

        var command = request.ToCommand();

        var response = await commandDispatcher.SendAsync<UploadDocumentCommand, Modules.Documents.Application.DTOs.UploadDocumentResponse>(
            command, cancellationToken);

        var contractResponse = new Contracts.Modules.Documents.DTOs.UploadDocumentResponse(
            response.DocumentId,
            response.UploadUrl,
            response.BlobName,
            response.ExpiresAt);

        return Results.Ok(Result<Contracts.Modules.Documents.DTOs.UploadDocumentResponse>.Success(contractResponse));
    }
}
