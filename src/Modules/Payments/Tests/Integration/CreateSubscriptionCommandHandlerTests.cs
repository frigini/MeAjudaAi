using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Exceptions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Payments.Tests.Integration;

public class CreateSubscriptionCommandHandlerTests : PaymentsIntegrationTestBase
{
    [Fact]
    public async Task Create_WithValidData_ShouldReturnCheckoutUrl()
    {
        var providerId = Guid.NewGuid();

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateSubscriptionCommand, string>>();
        var command = new CreateSubscriptionCommand(providerId, "basic");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("checkout.stripe.com");
    }

    [Fact]
    public async Task Create_WithInvalidPlan_ShouldThrowException()
    {
        var providerId = Guid.NewGuid();

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateSubscriptionCommand, string>>();
        var command = new CreateSubscriptionCommand(providerId, "nonexistent");

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<SubscriptionCreationException>();
    }

    [Fact]
    public async Task Create_WithIdempotencyKey_ShouldPersist()
    {
        var providerId = Guid.NewGuid();
        var idempotencyKey = $"idem_{Guid.NewGuid()}";

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateSubscriptionCommand, string>>();
        var command = new CreateSubscriptionCommand(providerId, "basic", idempotencyKey);

        await handler.HandleAsync(command, CancellationToken.None);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var subscription = await db.Subscriptions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProviderId == providerId);
        subscription.Should().NotBeNull();
        subscription!.PlanId.Should().Be("basic");
    }
}
