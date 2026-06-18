using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

public class GetDocumentStatusEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{documentId:guid}/status", GetDocumentStatusAsync)
            .WithName("GetDocumentStatus")
            .WithSummary("Obter status do documento")
            .WithDescription("""
                Retorna o status atual de um documento.

                **Status possíveis:**
                - Uploaded: Documento enviado, aguardando verificação
                - PendingVerification: Verificação solicitada
                - Verified: Documento verificado e aprovado
                - Rejected: Documento rejeitado
                - Failed: Falha no processamento
                """)
            .Produces<Result<ModuleDocumentStatusDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> GetDocumentStatusAsync(
        Guid documentId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = documentId.ToQuery();
        var result = await queryDispatcher.QueryAsync<GetDocumentByIdQuery, Application.DTOs.DocumentDto?>(
            query, cancellationToken);

        if (result is null)
            return NotFound("Documento não encontrado");

        var statusDto = new ModuleDocumentStatusDto(
            result.Id,
            result.Status.ToString(),
            result.VerifiedAt ?? result.UploadedAt);

        return Results.Ok(Result<ModuleDocumentStatusDto>.Success(statusDto));
    }
}
