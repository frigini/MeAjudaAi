using MeAjudaAi.Modules.Catalogs.Application.Commands.Service;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.DTOs.Requests;
using MeAjudaAi.Modules.Catalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Contracts.Modules.Catalogs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints;

// ============================================================
// CREATE
// ============================================================

public class CreateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", CreateAsync)
            .WithName("CreateService")
            .WithSummary("Criar serviço")
            .Produces<Response<ServiceDto>>(StatusCodes.Status201Created)
            .RequireAdmin();

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateServiceRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCommand(request.CategoryId, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<CreateServiceCommand, Result<ServiceDto>>(command, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        return Handle(result, "GetServiceById", new { id = result.Value!.Id });
    }
}

// ============================================================
// READ
// ============================================================

public class GetAllServicesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", GetAllAsync)
            .WithName("GetAllServices")
            .WithSummary("Listar todos os serviços")
            .Produces<Response<IReadOnlyList<ServiceListDto>>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetAllAsync(
        [AsParameters] GetAllServicesQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.QueryAsync<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>(
            query, cancellationToken);
        return Handle(result);
    }
}

public class GetServiceByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetServiceById")
            .WithSummary("Buscar serviço por ID")
            .Produces<Response<ServiceDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceByIdQuery(id);
        var result = await queryDispatcher.QueryAsync<GetServiceByIdQuery, Result<ServiceDto?>>(query, cancellationToken);

        if (result.IsSuccess && result.Value is null)
        {
            return Results.NotFound();
        }

        return Handle(result);
    }
}

public class GetServicesByCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/category/{categoryId:guid}", GetByCategoryAsync)
            .WithName("GetServicesByCategory")
            .WithSummary("Listar serviços por categoria")
            .Produces<Response<IReadOnlyList<ServiceListDto>>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetByCategoryAsync(
        Guid categoryId,
        [AsParameters] GetServicesByCategoryQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var queryWithCategory = query with { CategoryId = categoryId };
        var result = await queryDispatcher.QueryAsync<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>(queryWithCategory, cancellationToken);
        return Handle(result);
    }
}

// ============================================================
// UPDATE
// ============================================================

public class UpdateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateService")
            .WithSummary("Atualizar serviço")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateServiceRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCommand(id, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<UpdateServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}

public class ChangeServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/change-category", ChangeAsync)
            .WithName("ChangeServiceCategory")
            .WithSummary("Alterar categoria do serviço")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> ChangeAsync(
        Guid id,
        [FromBody] ChangeServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ChangeServiceCategoryCommand(id, request.NewCategoryId);
        var result = await commandDispatcher.SendAsync<ChangeServiceCategoryCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}

// ============================================================
// DELETE
// ============================================================

public class DeleteServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteService")
            .WithSummary("Deletar serviço")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeleteServiceCommand(id);
        var result = await commandDispatcher.SendAsync<DeleteServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}

// ============================================================
// ACTIVATE / DEACTIVATE
// ============================================================

public class ActivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateService")
            .WithSummary("Ativar serviço")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ActivateServiceCommand(id);
        var result = await commandDispatcher.SendAsync<ActivateServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}

public class DeactivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateService")
            .WithSummary("Desativar serviço")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAdmin();

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceCommand(id);
        var result = await commandDispatcher.SendAsync<DeactivateServiceCommand, Result>(command, cancellationToken);
        return HandleNoContent(result);
    }
}

// ============================================================
// VALIDATE
// ============================================================

public class ValidateServicesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/validate", ValidateAsync)
            .WithName("ValidateServices")
            .WithSummary("Validar múltiplos serviços")
            .Produces<Response<ValidateServicesResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();

    private static async Task<IResult> ValidateAsync(
        [FromBody] ValidateServicesRequest request,
        [FromServices] ICatalogsModuleApi moduleApi,
        CancellationToken cancellationToken)
    {
        var result = await moduleApi.ValidateServicesAsync(request.ServiceIds, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        var response = new ValidateServicesResponse(
            result.Value!.AllValid,
            result.Value.InvalidServiceIds,
            result.Value.InactiveServiceIds
        );

        return Handle(Result<ValidateServicesResponse>.Success(response));
    }
}
