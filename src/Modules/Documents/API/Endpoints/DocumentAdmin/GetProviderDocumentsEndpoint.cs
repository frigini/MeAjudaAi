using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Documents.API.Endpoints.DocumentAdmin;

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
            .WithName("GetProviderDocuments")
            .WithSummary("Listar documentos de um prestador")
            .WithDescription("""
                Retorna todos os documentos associados a um prestador específico.
                
                **Casos de uso:**
                - Visualizar todos os documentos enviados
                - Verificar status de verificação de documentos
                - Acompanhar progresso de validação de cadastro
                """)
            .Produces<IEnumerable<DocumentDto>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetProviderDocumentsAsync(
        Guid providerId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetProviderDocumentsQuery(providerId);
        var result = await queryDispatcher.QueryAsync<GetProviderDocumentsQuery, IEnumerable<DocumentDto>>(
            query, cancellationToken);

        return Results.Ok(result);
    }
}
