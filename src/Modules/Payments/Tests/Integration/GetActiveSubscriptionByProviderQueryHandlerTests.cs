using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Payments.Tests.Integration;

public class GetActiveSubscriptionByProviderQueryHandlerTests : PaymentsIntegrationTestBase
{
    [Fact]
    public async Task Get_ExistingActiveSubscription_ShouldReturnDto()
    {
        var providerId = Guid.NewGuid();
        await CreateSubscriptionAsync(providerId, "basic", externalId: $"sub_ext_{providerId}");

        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>>();
        var query = new GetActiveSubscriptionByProviderQuery(providerId, Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task Get_NonExistingSubscription_ShouldReturnNull()
    {
        using var scope = CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>>();
        var query = new GetActiveSubscriptionByProviderQuery(Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
