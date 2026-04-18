using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Contracts.Shared;
using FluentAssertions;
using Xunit;
using DomainOutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;
using ECommunicationChannel = MeAjudaAi.Modules.Communications.Domain.Enums.ECommunicationChannel;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Persistence;

public class OutboxMessageRepositoryTests
{
    [Fact]
    public async Task GetPendingAsync_ShouldMarkAsProcessing()
    {
        // Arrange
        using var context = CommunicationsTestDb.CreateSqlite();
        var repo = new OutboxMessageRepository(context);
        var msg = DomainOutboxMessage.Create(ECommunicationChannel.Email, "{}");
        await repo.AddAsync(msg);
        await repo.SaveChangesAsync();

        // Act
        var pending = await repo.GetPendingAsync(1);

        // Assert
        pending.Should().ContainSingle();
        pending[0].Status.Should().Be(EOutboxMessageStatus.Processing);
    }

    [Fact]
    public async Task GetPendingAsync_ShouldRespectRetryWindow_AndBatchSize()
    {
        // Arrange (Given)
        using var ctx = CommunicationsTestDb.CreateSqlite();
        var repo = new OutboxMessageRepository(ctx);
        var utcNow = DateTime.UtcNow;

        // 1. Pending and ready
        var msg1 = DomainOutboxMessage.Create(ECommunicationChannel.Email, "{}");
        
        // 2. Scheduled for future
        var msg2 = DomainOutboxMessage.Create(ECommunicationChannel.Email, "{}", scheduledAt: utcNow.AddHours(1));
        
        // 3. Already processed
        var msg3 = DomainOutboxMessage.Create(ECommunicationChannel.Email, "{}");
        msg3.MarkAsSent(utcNow);

        await repo.AddAsync(msg1);
        await repo.AddAsync(msg2);
        await repo.AddAsync(msg3);
        await repo.SaveChangesAsync();

        // Act (When)
        var result = await repo.GetPendingAsync(batchSize: 20);

        // Assert (Then)
        result.Should().ContainSingle();
        result[0].Id.Should().Be(msg1.Id);
    }

    [Fact]
    public async Task CleanupOldMessagesAsync_ShouldRemoveSentMessagesBeforeThreshold()
    {
        // Arrange
        using var context = CommunicationsTestDb.CreateSqlite();
        var repo = new OutboxMessageRepository(context);
        var oldMsg = DomainOutboxMessage.Create(ECommunicationChannel.Email, "{}");
        oldMsg.MarkAsSent(DateTime.UtcNow);
        
        typeof(BaseEntity).GetProperty("CreatedAt")?.SetValue(oldMsg, DateTime.UtcNow.AddDays(-10));
        
        await repo.AddAsync(oldMsg);
        await repo.SaveChangesAsync();

        // Act
        var deletedCount = await repo.CleanupOldMessagesAsync(DateTime.UtcNow.AddDays(-5));

        // Assert
        deletedCount.Should().Be(1);
        context.OutboxMessages.Should().BeEmpty();
    }
}
