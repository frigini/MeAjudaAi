using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Payments.Tests.Integration;

public class CreateBillingPortalSessionCommandHandlerTests : PaymentsIntegrationTestBase
{
    [Fact]
    public async Task Create_WithValidSubscription_ShouldReturnPortalUrl()
    {
        var providerId = Guid.NewGuid();
        await CreateSubscriptionAsync(providerId, "basic", externalId: $"sub_ext_{providerId}");

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateBillingPortalSessionCommand, string>>();
        var command = new CreateBillingPortalSessionCommand(providerId, "https://return.test.com");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("billing.stripe.com");
    }

    [Fact]
    public async Task Create_WithoutSubscription_ShouldThrowNotFoundException()
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<CreateBillingPortalSessionCommand, string>>();
        var command = new CreateBillingPortalSessionCommand(Guid.NewGuid(), "https://return.test.com");

        var act = () => handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
