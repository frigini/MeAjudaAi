using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.ApiService.Endpoints;

/// <summary>
/// Endpoints públicos de registro de prestadores de serviços.
/// Orquestra a criação de usuário (módulo Users) + entidade Provider (módulo Providers).
/// Fica no ApiService pois é o único projeto que referencia ambos os módulos.
/// </summary>
public static class ProviderRegistrationEndpoints
{
    public static IEndpointRouteBuilder MapProviderRegistrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/providers")
            .WithTags("Providers - Public");

        group.MapPost("/register", RegisterProviderAsync)
            .WithName("RegisterProvider")
            .WithSummary("Auto-registro de prestador de serviços")
            .WithDescription(
                "Inicia o cadastro de um prestador. Cria usuário no Keycloak com role 'provider-standard' " +
                "e a entidade Provider com Tier=Standard. Endpoint público, sem autenticação.")
            .Produces<Response<ProviderDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> RegisterProviderAsync(
        [FromBody] RegisterProviderRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (!request.AcceptedTerms || !request.AcceptedPrivacyPolicy)
            return Results.BadRequest("Você deve aceitar os Termos de Uso e a Política de Privacidade para se cadastrar.");

        // Passo 1: Criar usuário no Keycloak com role provider-standard (módulo Users)
        var phone = request.PhoneNumber
            .Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        var username = $"provider_{phone}";

        var createUserCommand = new CreateUserCommand(
            Username: username,
            Email: request.Email,
            FirstName: request.Name,
            LastName: string.Empty,
            Password: Guid.NewGuid().ToString("N"), // Senha temporária — login via Keycloak/social
            Roles: [UserRoles.ProviderStandard],
            PhoneNumber: request.PhoneNumber
        );

        var userResult = await commandDispatcher.SendAsync<CreateUserCommand, Result<MeAjudaAi.Modules.Users.Application.DTOs.UserDto>>(
            createUserCommand, cancellationToken);

        if (userResult.IsFailure)
            return Results.BadRequest(userResult.Error.Message);

        // Passo 2: Criar entidade Provider vinculada ao usuário (módulo Providers)
        var createProviderCommand = new CreateProviderCommand(
            UserId: userResult.Value!.Id,
            Name: request.Name,
            Type: request.Type,
            BusinessProfile: new BusinessProfileDto(
                LegalName: request.Name,
                FantasyName: null,
                Description: null,
                ContactInfo: new ContactInfoDto(
                    Email: request.Email,
                    PhoneNumber: request.PhoneNumber,
                    Website: null),
                PrimaryAddress: new AddressDto(
                    Street: string.Empty,
                    Number: string.Empty,
                    Complement: null,
                    Neighborhood: string.Empty,
                    City: string.Empty,
                    State: string.Empty,
                    ZipCode: string.Empty,
                    Country: "BR")
            )
        );

        var providerResult = await commandDispatcher.SendAsync<CreateProviderCommand, Result<ProviderDto>>(
            createProviderCommand, cancellationToken);

        if (providerResult.IsFailure)
            return Results.BadRequest(providerResult.Error.Message);

        return Results.Created(
            $"/api/v1/providers/{providerResult.Value!.Id}",
            new Response<ProviderDto>(providerResult.Value));
    }
}
