using FluentAssertions;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Shared;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MeAjudaAi.Shared.Tests.Unit.Database.Outbox;

public class OutboxRepositoryTests
{
    private readonly DbContextOptions<BaseDbContext> _options;
    
    public OutboxRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<BaseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private class TestDbContext : BaseDbContext
    {
        public TestDbContext(DbContextOptions options) : base(options) { }
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyPendingAndOrderedCorrectly()
    {
        using var context = new TestDbContext(_options);
        var repo = new OutboxRepository<OutboxMessage>(context);
        var now = DateTime.UtcNow;

        var m1 = OutboxMessage.Create("Type1", "Payload1", ECommunicationPriority.Normal, null);
        var m2 = OutboxMessage.Create("Type2", "Payload2", ECommunicationPriority.High, null);
        var m3 = OutboxMessage.Create("Type3", "Payload3", ECommunicationPriority.Low, now.AddMinutes(10));
        
        context.OutboxMessages.AddRange(m1, m2, m3);
        await context.SaveChangesAsync();

        var pending = await repo.GetPendingAsync(batchSize: 10, utcNow: now);

        pending.Should().HaveCount(2);
        pending[0].Priority.Should().Be(ECommunicationPriority.High);
        pending[1].Priority.Should().Be(ECommunicationPriority.Normal);
    }
}
