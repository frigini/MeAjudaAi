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
    public async Task GetPendingAsync_ShouldExcludeProcessedAndFutureMessages()
    {
        using var context = new TestDbContext(_options);
        var repo = new OutboxRepository<OutboxMessage>(context);
        var now = DateTime.UtcNow;

        var mPending = OutboxMessage.Create("Type1", "P1", ECommunicationPriority.Normal, null);
        var mProcessing = OutboxMessage.Create("Type2", "P2", ECommunicationPriority.Normal, null);
        mProcessing.MarkAsSent(now); // Marca como enviado/processado
        var mScheduled = OutboxMessage.Create("Type3", "P3", ECommunicationPriority.Normal, now.AddMinutes(10));
        
        context.OutboxMessages.AddRange(mPending, mProcessing, mScheduled);
        await context.SaveChangesAsync();

        var pending = await repo.GetPendingAsync(10, now);

        pending.Should().HaveCount(1);
        pending.Should().Contain(m => m.Id == mPending.Id);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldRespectBatchSize()
    {
        using var context = new TestDbContext(_options);
        var repo = new OutboxRepository<OutboxMessage>(context);
        
        for (int i = 0; i < 5; i++)
            context.OutboxMessages.Add(OutboxMessage.Create("T", "P", ECommunicationPriority.Normal, null));
        await context.SaveChangesAsync();

        var pending = await repo.GetPendingAsync(3);

        pending.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldUseCreatedAtAsTiebreaker()
    {
        using var context = new TestDbContext(_options);
        var repo = new OutboxRepository<OutboxMessage>(context);
        
        var m1 = OutboxMessage.Create("T", "P1", ECommunicationPriority.Normal, null);
        var m2 = OutboxMessage.Create("T", "P2", ECommunicationPriority.Normal, null);
        
        context.OutboxMessages.AddRange(m1, m2);
        await context.SaveChangesAsync();

        var pending = await repo.GetPendingAsync();

        pending.Should().HaveCount(2);
        pending[0].CreatedAt.Should().BeBefore(pending[1].CreatedAt);
    }
}

