using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Documents.DTOs;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Mappers;
using MeAjudaAi.Modules.Documents.API.Mappers;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela listagem de documentos de um prestador.
/// </summary>
public class GetProviderDocumentsEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de listagem de documentos.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/provider/{providerId:guid}", GetProviderDocumentsAsync)
            .WithName(ApiEndpoints.Documents.Names.GetByProvider)
            .WithSummary("Listar documentos de um prestador")
            .WithDescription("""
                Retorna todos os documentos associados a um prestador específico.
                
                **Casos de uso:**
                - Visualizar todos os documentos enviados
                - Verificar status de verificação de documentos
                - Acompanhar progresso de validação de cadastro
                """)
            .Produces<Result<IReadOnlyList<ModuleDocumentDto>>>(StatusCodes.Status200OK)
            .WithTags(DocumentsEndpoints.Tag);

    private static async Task<IResult> GetProviderDocumentsAsync(
        Guid providerId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = providerId.ToDocumentsQuery();
        var result = await queryDispatcher.QueryAsync<Application.Queries.GetProviderDocumentsQuery, IEnumerable<DocumentDto>>(
            query, cancellationToken);

        var contractResult = result.Select(d => d.ToModuleDto()).ToList();

        return Results.Ok(Result<IReadOnlyList<ModuleDocumentDto>>.Success(contractResult));
    }
}
