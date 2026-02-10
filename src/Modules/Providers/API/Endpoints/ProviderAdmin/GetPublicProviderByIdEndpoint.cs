using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint público para consulta de detalhes básicos do prestador.
/// </summary>
public class GetPublicProviderByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}/public", GetPublicProviderAsync)
            .WithName("GetPublicProviderById")
            .WithSummary("Consultar perfil público do prestador")
            .WithDescription("""
                Recupera dados públicos e seguros de um prestador para exibição no site.
                Não requer autenticação.
                
                **Dados Retornados:**
                - Informações básicas (Nome, Fantasia, Descrição)
                - Localização aproximada (Cidade/Estado)
                - Avaliação média e contagem de reviews
                - Lista de serviços oferecidos
                
                **Dados Ocultados (Privacidade):**
                - Documentos (CPF/CNPJ)
                - Endereço completo (Rua/Número)
                - Dados de auditoria interna
                """)
            .AllowAnonymous() // Permite acesso sem login
            .Produces<Response<PublicProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetPublicProviderAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetPublicProviderByIdQuery(id);
        var result = await queryDispatcher.QueryAsync<GetPublicProviderByIdQuery, Result<PublicProviderDto?>>(
            query, cancellationToken);

        return Handle(result);
    }
}
