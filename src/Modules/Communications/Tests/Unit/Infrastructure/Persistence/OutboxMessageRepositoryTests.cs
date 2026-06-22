using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Communications;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Persistence;

public class OutboxMessageRepositoryTests : BaseInMemoryDatabaseTest<CommunicationsDbContext>
{
    public OutboxMessageRepositoryTests() : base(options => new CommunicationsDbContext(options))
    {
    }

    [Fact]
    public async Task GetPendingAsync_ShouldMarkAsProcessing()
    {
        var repo = new OutboxMessageRepository(DbContext);
        var msg = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .Build();
        await repo.AddAsync(msg);
        await repo.SaveChangesAsync();

        var pending = await repo.GetPendingAsync(1);

        pending.Should().ContainSingle();
        pending[0].Status.Should().Be(EOutboxMessageStatus.Processing);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldRespectRetryWindow_AndBatchSize()
    {
        var repo = new OutboxMessageRepository(DbContext);
        var utcNow = DateTime.UtcNow;

        var msg1 = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .Build();

        var msg2 = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .AsScheduled(utcNow.AddHours(1))
            .Build();

        var msg3 = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .Build();
        msg3.MarkAsSent(utcNow);

        await repo.AddAsync(msg1);
        await repo.AddAsync(msg2);
        await repo.AddAsync(msg3);
        await repo.SaveChangesAsync();

        var result = await repo.GetPendingAsync(batchSize: 20);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(msg1.Id);
    }

    [Fact]
    public async Task CleanupOldMessagesAsync_ShouldRemoveSentMessagesBeforeThreshold()
    {
        var repo = new OutboxMessageRepository(DbContext);
        var oldMsg = new OutboxMessageBuilder()
            .WithChannel(ECommunicationChannel.Email)
            .WithPayload("{}")
            .Build();
        oldMsg.MarkAsSent(DateTime.UtcNow);

        typeof(BaseEntity).GetProperty("CreatedAt")?.SetValue(oldMsg, DateTime.UtcNow.AddDays(-10));

        await repo.AddAsync(oldMsg);
        await repo.SaveChangesAsync();

        var deletedCount = await repo.CleanupOldMessagesAsync(DateTime.UtcNow.AddDays(-5));

        deletedCount.Should().Be(1);
        DbContext.OutboxMessages.Should().BeEmpty();
    }
}