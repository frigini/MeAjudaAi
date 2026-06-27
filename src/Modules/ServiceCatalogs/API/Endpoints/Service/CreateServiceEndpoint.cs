using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.Service;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para criar um novo serviço no catálogo.
/// Requer privilégios de administrador.
/// </summary>
public class CreateServiceEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint POST / para criar um serviço.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.ServiceCatalogs.Services.Create, CreateAsync)
            .WithName("CreateService")
            .WithSummary("Criar serviço")
            .WithDescription("""
                Cria um novo serviço no catálogo.
                
                **Validações:**
                - Nome é obrigatório (máximo 150 caracteres)
                - Descrição opcional (máximo 1000 caracteres)
                - DisplayOrder deve ser >= 0
                - Categoria deve existir e estar ativa
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces<Response<ServiceDto>>(StatusCodes.Status201Created)
            .RequireAdmin();

    /// <summary>
    /// Cria um novo serviço a partir dos dados fornecidos.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        [FromBody] CreateServiceRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCommand(request.CategoryId, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<CreateServiceCommand, Result<ServiceDto>>(command, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        return Handle(result, "GetServiceById", new { id = result.Value.Id });
    }
}
