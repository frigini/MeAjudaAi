using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela consulta de status de um documento.
/// </summary>
public class GetDocumentStatusEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de status.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{documentId:guid}/status", GetDocumentStatusAsync)
            .WithName("GetDocumentStatus")
            .WithSummary("Consultar status de documento")
            .WithDescription("""
                Retorna informações detalhadas sobre um documento específico.
                
                **Informações retornadas:**
                - Status atual (Uploaded, PendingVerification, Verified, Rejected, Failed)
                - Datas de upload e verificação
                - Motivo de rejeição (se aplicável)
                - Dados extraídos por OCR (se disponível)
                - URLs de acesso ao documento
                """)
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> GetDocumentStatusAsync(
        Guid documentId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetDocumentStatusQuery(documentId);
        var result = await queryDispatcher.QueryAsync<GetDocumentStatusQuery, DocumentDto?>(
            query, cancellationToken);

        if (result == null)
            return NotFound("Documento não encontrado");

        return Results.Ok(result);
    }
}
