using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            .AllowAnonymous()
            .RequireRateLimiting("provider-registration");

        return endpoints;
    }

    private static async Task<IResult> RegisterProviderAsync(
        [FromBody] RegisterProviderRequest request,
        ICommandDispatcher commandDispatcher,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(ProviderRegistrationEndpoints).FullName!);

        if (!request.AcceptedTerms || !request.AcceptedPrivacyPolicy)
            return Results.BadRequest("Você deve aceitar os Termos de Uso e a Política de Privacidade para se cadastrar.");

        // Passo 1: Criar usuário no Keycloak com role provider-standard (módulo Users)
        // Sanitiza telefone mantendo apenas números
        var phone = System.Text.RegularExpressions.Regex.Replace(request.PhoneNumber, @"\D", "");
        var username = $"provider_{phone}";

        // Use First Name as placeholder for Last Name to satisfy validation
        var nameParts = request.Name.Trim().Split(' ', 2);
        var firstName = nameParts[0];
        var lastName = nameParts.Length > 1 ? nameParts[1] : firstName; // Fallback to firstname if no lastname

        var createUserCommand = new CreateUserCommand(
            Username: username,
            Email: request.Email,
            FirstName: firstName,
            LastName: lastName,
            Password: GenerateTemporaryPassword(), // Senha temporária forte gerada dinamicamente
            Roles: [UserRoles.ProviderStandard],
            PhoneNumber: request.PhoneNumber
        );

        var userResult = await commandDispatcher.SendAsync<CreateUserCommand, Result<MeAjudaAi.Modules.Users.Application.DTOs.UserDto>>(
            createUserCommand, cancellationToken);

        if (userResult.IsFailure)
        {
            // Logar erro detalhado internamente
            logger.LogError("Failed to create Keycloak user for provider registration. Error: {Error}", userResult.Error.Message);
            return Results.BadRequest("Ocorreu um erro ao registrar o usuário.");
        }

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
        {
            // Compensação: Tentar remover o usuário criado para evitar orfãos
            try 
            {
                var deleteUserCommand = new DeleteUserCommand(userResult.Value!.Id);
                await commandDispatcher.SendAsync<DeleteUserCommand, Result>(deleteUserCommand, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Compensation failed: Could not delete orphaned user {UserId} after provider creation failed.", userResult.Value!.Id);
            }

            return Results.BadRequest("Ocorreu um erro ao registrar o provedor.");
        }

        return Results.Created(
            $"/api/v1/providers/{providerResult.Value!.Id}",
            new Response<ProviderDto>(providerResult.Value));
    }

    private static string GenerateTemporaryPassword()
    {
        // Gera uma senha forte aleatória que satisfaz requisitos do Keycloak (Maiúscula, Minúscula, Número, Especial)
        return $"Temp{Guid.NewGuid().ToString("N")[..8]}!123";
    }
}
