using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Mappers;
using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela consulta de um documento por ID.
/// </summary>
public class GetDocumentByIdEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de documento por ID.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{documentId:guid}", GetDocumentByIdAsync)
            .WithName("GetDocumentById")
            .WithSummary("Obter documento por ID")
            .WithDescription("""
                Retorna informações detalhadas sobre um documento específico.
                
                **Informações retornadas:**
                - ID do documento e do provider
                - Tipo e nome do arquivo
                - Status atual (Uploaded, PendingVerification, Verified, Rejected, Failed)
                - Datas de upload e verificação
                - Dados extraídos por OCR (se disponível)
                """)
            .Produces<Result<ModuleDocumentDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> GetDocumentByIdAsync(
        Guid documentId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = documentId.ToQuery();
        var result = await queryDispatcher.QueryAsync<Application.Queries.GetDocumentStatusQuery, DocumentDto?>(
            query, cancellationToken);

        if (result is null)
            return NotFound("Documento não encontrado");

        var contractResult = result.ToModuleDto();

        return Results.Ok(Result<ModuleDocumentDto>.Success(contractResult));
    }
}
