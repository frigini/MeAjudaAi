using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

/// <summary>
/// Endpoint para criar uma nova categoria de serviços.
/// Requer privilégios de administrador.
/// </summary>
public class CreateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint POST / para criar uma categoria.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.ServiceCatalogs.Categories.Create, CreateAsync)
            .WithName("CreateServiceCategory")
            .WithSummary("Criar categoria de serviço")
            .WithDescription("""
                Cria uma nova categoria de serviços no catálogo.
                
                **Validações:**
                - Nome é obrigatório (máximo 100 caracteres)
                - Descrição opcional (máximo 500 caracteres)
                - DisplayOrder deve ser >= 0
                - Nome deve ser único no sistema
                
                **Efeitos:**
                - Categoria criada como ativa por padrão
                - Pode receber serviços imediatamente
                
                **Permissões:** Requer privilégios de administrador
                """)
            .Produces<Response<ServiceCategoryDto>>(StatusCodes.Status201Created)
            .RequireAdmin();

    /// <summary>
    /// Cria uma nova categoria de serviço a partir dos dados fornecidos.
    /// </summary>
    private static async Task<IResult> CreateAsync(
        [FromBody] CreateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCategoryCommand(request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<CreateServiceCategoryCommand, Result<ServiceCategoryDto>>(
            command, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        if (result.Value is null)
            return Results.BadRequest("Unexpected null value in successful result.");

        return Handle(result, "GetServiceCategoryById", new { id = result.Value.Id });
    }
}
