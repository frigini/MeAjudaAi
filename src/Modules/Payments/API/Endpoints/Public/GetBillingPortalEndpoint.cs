using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Payments.DTOs;
using MeAjudaAi.Modules.Payments.API.Helpers;
using MeAjudaAi.Modules.Payments.API.Mappers;
using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.Services;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public class GetBillingPortalEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Payments.GetBillingPortal, GetBillingPortalAsync)
        .RequireAuthorization()
        .Produces<GetBillingPortalResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(PaymentsEndpoints.Tag)
        .WithName("GetBillingPortal")
        .WithSummary("Gera link do portal de faturamento")
        .WithDescription("Gera um link para o portal de gerenciamento de faturamento do Stripe.");
    }

    private static async Task<IResult> GetBillingPortalAsync(
        GetBillingPortalRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        [FromServices] IReturnUrlResolver returnUrlResolver,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (request.ProviderId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ProviderId"] = ["O campo ProviderId é obrigatório."]
            });
        }

        var authResult = PaymentAuthorizationHelper.AuthorizeProviderAccess(httpContext, request.ProviderId);
        if (authResult is not null)
        {
            return authResult;
        }

        var resolveResult = returnUrlResolver.Resolve(request.ReturnUrl, request.ProviderId);
        if (resolveResult.IsFailure)
        {
            return resolveResult.Error.ToProblem();
        }

        var command = request.ToCommand(resolveResult.Value!);
        var portalUrl = await dispatcher.SendAsync<CreateBillingPortalSessionCommand, string>(command, cancellationToken);

        return Results.Ok(new GetBillingPortalResponse(portalUrl));
    }
}
