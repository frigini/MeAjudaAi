using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Contracts.Shared;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Shared.Tests.Unit.Database.Outbox;

public class OutboxRepositoryTests
{
    private class TestOutboxDbContext : DbContext
    {
        public TestOutboxDbContext(DbContextOptions<TestOutboxDbContext> options) : base(options) { }

        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutboxMessage>().HasKey(m => m.Id);
        }
    }

    private TestOutboxDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestOutboxDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestOutboxDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddMessageToDbSet()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new OutboxRepository<OutboxMessage>(context);
        var message = OutboxMessage.Create("TestType", "TestPayload");

        // Act
        await repository.AddAsync(message);
        await repository.SaveChangesAsync();

        // Assert
        var dbMessage = await context.OutboxMessages.FindAsync(message.Id);
        dbMessage.Should().NotBeNull();
        dbMessage!.Type.Should().Be("TestType");
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnMessagesOrderedByPriorityThenScheduledAtThenCreatedAt()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new OutboxRepository<OutboxMessage>(context);

        var now = DateTime.UtcNow;

        // Message 1: Normal Priority, scheduled now
        var m1 = OutboxMessage.Create("Type1", "Payload1", ECommunicationPriority.Normal);
        // Message 2: High Priority, scheduled now
        var m2 = OutboxMessage.Create("Type2", "Payload2", ECommunicationPriority.High);
        // Message 3: Low Priority, scheduled now
        var m3 = OutboxMessage.Create("Type3", "Payload3", ECommunicationPriority.Low);
        // Message 4: High Priority, scheduled in future
        var m4 = OutboxMessage.Create("Type4", "Payload4", ECommunicationPriority.High, scheduledAt: now.AddMinutes(10));
        // Message 5: Normal Priority, sent already
        var m5 = OutboxMessage.Create("Type5", "Payload5", ECommunicationPriority.Normal);
        m5.MarkAsSent(now);

        await repository.AddAsync(m1);
        await repository.AddAsync(m2);
        await repository.AddAsync(m3);
        await repository.AddAsync(m4);
        await repository.AddAsync(m5);
        await repository.SaveChangesAsync();

        // Act
        var pending = await repository.GetPendingAsync(batchSize: 10, utcNow: now);

        // Assert
        // Should only return m2, m1, m3 in that order (High -> Normal -> Low)
        // m4 is in future, m5 is sent
        pending.Should().HaveCount(3);
        pending[0].Id.Should().Be(m2.Id);
        pending[1].Id.Should().Be(m1.Id);
        pending[2].Id.Should().Be(m3.Id);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldRespectBatchSize()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new OutboxRepository<OutboxMessage>(context);

        for (int i = 0; i < 5; i++)
        {
            await repository.AddAsync(OutboxMessage.Create($"Type{i}", "Payload"));
        }
        await repository.SaveChangesAsync();

        // Act
        var pending = await repository.GetPendingAsync(batchSize: 3);

        // Assert
        pending.Should().HaveCount(3);
    }
}
