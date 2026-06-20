using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Queries;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Payments;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Infrastructure")]
public class DbContextSubscriptionQueriesTests : BaseInMemoryDatabaseTest<PaymentsDbContext>
{
    private readonly DbContextSubscriptionQueries _queries;

    public DbContextSubscriptionQueriesTests()
        : base(options => new PaymentsDbContext(options))
    {
        _queries = new DbContextSubscriptionQueries(DbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingSubscription_ShouldReturnSubscription()
    {
        var sub = new SubscriptionBuilder()
            .WithPlanId("premium")
            .WithAmount(MoneyBuilder.Brl(99.90m))
            .Build();
        DbContext.Subscriptions.Add(sub);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetByIdAsync(sub.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(sub.Id);
    }

    [Fact]
    public async Task GetByExternalIdAsync_WithExistingSubscription_ShouldReturnSubscription()
    {
        var sub = new SubscriptionBuilder()
            .WithPlanId("premium")
            .WithAmount(MoneyBuilder.Brl(99.90m))
            .Build();
        sub.Activate("ext-sub-123", "cus-123", DateTime.UtcNow.AddMonths(1));
        DbContext.Subscriptions.Add(sub);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetByExternalIdAsync("ext-sub-123");

        result.Should().NotBeNull();
        result!.ExternalSubscriptionId.Should().Be("ext-sub-123");
    }

    [Fact]
    public async Task GetLatestByProviderIdAsync_ShouldReturnMostRecentSubscription()
    {
        var providerId = Guid.NewGuid();
        var oldSub = new SubscriptionBuilder()
            .WithProviderId(providerId)
            .WithPlanId("basic")
            .WithAmount(MoneyBuilder.Brl(50.00m))
            .Build();
        var newSub = new Subscription(providerId, "premium", Money.FromDecimal(99.90m, "BRL"));

        DbContext.Subscriptions.Add(oldSub);
        DbContext.Subscriptions.Add(newSub);
        await DbContext.SaveChangesAsync();

        var result = await _queries.GetLatestByProviderIdAsync(providerId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(newSub.Id);
    }
}
