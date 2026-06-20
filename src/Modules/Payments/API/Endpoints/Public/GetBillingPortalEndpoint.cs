using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.API.Helpers;
using MeAjudaAi.Modules.Payments.API.Mappers;
using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.DTOs.Requests;
using MeAjudaAi.Modules.Payments.Application.DTOs.Responses;
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

    /// <summary>
    /// Gera um link para o portal de gerenciamento de faturamento do Stripe.
    /// </summary>
    /// <param name="request">Dados da requisição (prestador e URL de retorno opcional).</param>
    /// <param name="dispatcher">Disparador de comandos.</param>
    /// <param name="configuration">Configuração da aplicação para resolver ClientBaseUrl.</param>
    /// <param name="logger">Logger do endpoint.</param>
    /// <param name="httpContext">Contexto da requisição HTTP para extração de identidade.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>URL do portal de faturamento.</returns>
    private static async Task<IResult> GetBillingPortalAsync(
        GetBillingPortalRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        [FromServices] IConfiguration configuration,
        [FromServices] ILogger<GetBillingPortalEndpoint> logger,
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

        var resolveResult = ResolveReturnUrl(request.ReturnUrl, configuration, request.ProviderId, logger);
        if (resolveResult.IsFailure)
        {
            return resolveResult.Error.ToProblem();
        }

        var command = request.ToCommand(resolveResult.Value!);
        var portalUrl = await dispatcher.SendAsync<CreateBillingPortalSessionCommand, string>(command, cancellationToken);

        return Results.Ok(new GetBillingPortalResponse(portalUrl));
    }

    /// <summary>
    /// Resolve a URL de retorno do portal de faturamento com base no valor informado.
    /// </summary>
    /// <param name="returnUrl">URL de retorno informada pelo cliente.</param>
    /// <param name="configuration">Configuração da aplicação.</param>
    /// <param name="providerId">ID do prestador para logs.</param>
    /// <param name="logger">Logger do endpoint.</param>
    /// <returns>URL de retorno resolvida ou erro de configuração.</returns>
    private static Result<string> ResolveReturnUrl(
        string? returnUrl,
        IConfiguration configuration,
        Guid providerId,
        ILogger<GetBillingPortalEndpoint> logger)
    {
        var clientBaseUrl = configuration["ClientBaseUrl"];
        if (string.IsNullOrEmpty(clientBaseUrl))
        {
            return Result<string>.Failure(Error.Internal("ClientBaseUrl não configurada."));
        }

        clientBaseUrl = clientBaseUrl.TrimEnd('/');
        var normalizedReturnUrl = returnUrl ?? "";

        if (normalizedReturnUrl.Equals("account", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Success($"{clientBaseUrl}/account");
        }

        if (normalizedReturnUrl.Equals("billing", StringComparison.OrdinalIgnoreCase))
        {
            return Result<string>.Success($"{clientBaseUrl}/billing");
        }

        if (Uri.TryCreate(normalizedReturnUrl, UriKind.Absolute, out var validatedUri))
        {
            var allowedHost = new Uri(clientBaseUrl).Host;
            if (string.Equals(validatedUri.Host, allowedHost, StringComparison.OrdinalIgnoreCase))
            {
                return Result<string>.Success(normalizedReturnUrl);
            }
        }

        logger.LogInformation(
            "Billing portal ReturnUrl fallback taken for Provider {ProviderId}. Original value: {ReturnUrl}",
            providerId, normalizedReturnUrl);
        return Result<string>.Success(clientBaseUrl);
    }
}
