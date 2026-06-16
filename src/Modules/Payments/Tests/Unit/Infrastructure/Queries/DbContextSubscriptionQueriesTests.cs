using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Payments;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Infrastructure")]
public class DbContextSubscriptionQueriesTests : IDisposable
{
    private readonly PaymentsDbContext _dbContext;
    private readonly DbContextSubscriptionQueries _queries;

    public DbContextSubscriptionQueriesTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase("SubscriptionQueriesTest_" + Guid.NewGuid())
            .Options;
        _dbContext = new PaymentsDbContext(options);
        _queries = new DbContextSubscriptionQueries(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetByIdAsync_WithExistingSubscription_ShouldReturnSubscription()
    {
        var sub = new SubscriptionBuilder()
            .WithPlanId("premium")
            .WithAmount(MoneyBuilder.Brl(99.90m))
            .Build();
        _dbContext.Subscriptions.Add(sub);
        await _dbContext.SaveChangesAsync();

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
        _dbContext.Subscriptions.Add(sub);
        await _dbContext.SaveChangesAsync();

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
        
        _dbContext.Subscriptions.Add(oldSub);
        _dbContext.Subscriptions.Add(newSub);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetLatestByProviderIdAsync(providerId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(newSub.Id);
    }
}
