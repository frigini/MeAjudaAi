using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints;

// ============================================================
// Request DTOs
// ============================================================

public record CreateServiceCategoryRequest(string Name, string Description, int DisplayOrder);
public record UpdateServiceCategoryRequest(string Name, string Description, int DisplayOrder);

// ============================================================
// CREATE
// ============================================================

public class CreateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/", CreateAsync)
            .WithName("CreateServiceCategory")
            .WithSummary("Criar categoria de serviço")
            .Produces<Response<Guid>>(StatusCodes.Status201Created)
            .RequireAuthorization("Admin");

    private static async Task<IResult> CreateAsync(
        [FromBody] CreateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCategoryCommand(request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<CreateServiceCategoryCommand, Result<Guid>>(
            command, cancellationToken);

        if (!result.IsSuccess)
            return Handle(result);

        return Handle(result, "GetServiceCategoryById", new { id = result.Value });
    }
}

// ============================================================
// READ
// ============================================================

public class GetAllServiceCategoriesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", GetAllAsync)
            .WithName("GetAllServiceCategories")
            .WithSummary("Listar todas as categorias")
            .Produces<Response<IReadOnlyList<ServiceCategoryDto>>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetAllAsync(
        [AsParameters] GetAllCategoriesQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var qry = new GetAllServiceCategoriesQuery(query.ActiveOnly);
        var result = await queryDispatcher.QueryAsync<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>(
            qry, cancellationToken);

        return Handle(result);
    }
}

public record GetAllCategoriesQuery(bool ActiveOnly = false);

public class GetServiceCategoryByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetServiceCategoryById")
            .WithSummary("Buscar categoria por ID")
            .Produces<Response<ServiceCategoryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceCategoryByIdQuery(id);
        var result = await queryDispatcher.QueryAsync<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>(
            query, cancellationToken);

        if (result.IsSuccess && result.Value == null)
            return Results.NotFound();

        return Handle(result);
    }
}

// ============================================================
// UPDATE
// ============================================================

public class UpdateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateServiceCategory")
            .WithSummary("Atualizar categoria de serviço")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateServiceCategoryRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateServiceCategoryCommand(id, request.Name, request.Description, request.DisplayOrder);
        var result = await commandDispatcher.SendAsync<UpdateServiceCategoryCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}

// ============================================================
// DELETE
// ============================================================

public class DeleteServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteServiceCategory")
            .WithSummary("Deletar categoria de serviço")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeleteServiceCategoryCommand(id);
        var result = await commandDispatcher.SendAsync<DeleteServiceCategoryCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}

// ============================================================
// ACTIVATE / DEACTIVATE
// ============================================================

public class ActivateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateServiceCategory")
            .WithSummary("Ativar categoria de serviço")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ActivateServiceCategoryCommand(id);
        var result = await commandDispatcher.SendAsync<ActivateServiceCategoryCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}

public class DeactivateServiceCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateServiceCategory")
            .WithSummary("Desativar categoria de serviço")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceCategoryCommand(id);
        var result = await commandDispatcher.SendAsync<DeactivateServiceCategoryCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}
