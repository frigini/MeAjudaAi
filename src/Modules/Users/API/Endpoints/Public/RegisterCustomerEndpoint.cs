using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Public;

public class RegisterCustomerEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("register", async (
            RegisterCustomerRequest request,
            ICommandDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCustomerCommand(
                request.Name,
                request.Email,
                request.Password,
                request.PhoneNumber,
                request.TermsAccepted
            );

            var result = await dispatcher.SendAsync<RegisterCustomerCommand, Result<UserDto>>(command, cancellationToken);

            return EndpointExtensions.Handle(result);
        })
        .WithTags("Users")
        .WithSummary("Registers a new customer")
        .WithDescription("Creates a new user account with 'customer' role.")
        .RequireRateLimiting("registration")
        .AllowAnonymous(); // Endpoint público
    }
}

public record RegisterCustomerRequest(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    bool TermsAccepted
);
