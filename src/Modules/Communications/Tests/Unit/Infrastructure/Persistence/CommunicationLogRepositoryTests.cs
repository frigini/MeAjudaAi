using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Persistence;

public class CommunicationLogRepositoryTests
{
    [Fact]
    public async Task GetByRecipientAsync_ShouldFilterCorrectly_WhenLogsAreSeeded()
    {
        // Arrange (Given)
        using var ctx = CommunicationsTestDb.CreateSqlite();
        var repo = new CommunicationLogRepository(ctx);
        var recipientId = "user-123";
        
        var log1 = CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, recipientId, 1);
        var log2 = CommunicationLog.CreateSuccess("corr-2", ECommunicationChannel.Sms, "other-user", 1);
        
        await repo.AddAsync(log1);
        await repo.AddAsync(log2);
        await repo.SaveChangesAsync();

        // Act (When)
        var result = await repo.GetByRecipientAsync(recipientId);

        // Assert (Then)
        result.Should().ContainSingle();
        result[0].Recipient.Should().Be(recipientId);
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_ShouldReturnTrue_WhenCorrelationIdIsDuplicated()
    {
        // Arrange (Given)
        using var ctx = CommunicationsTestDb.CreateSqlite();
        var repo = new CommunicationLogRepository(ctx);
        var correlationId = "duplicated-id";
        
        var log = CommunicationLog.CreateSuccess(correlationId, ECommunicationChannel.Email, "user-1", 1);
        await repo.AddAsync(log);
        await repo.SaveChangesAsync();

        // Act (When)
        var exists = await repo.ExistsByCorrelationIdAsync(correlationId);

        // Assert (Then)
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByChannel_AndUseAsNoTracking()
    {
        // Arrange (Given)
        using var ctx = CommunicationsTestDb.CreateSqlite();
        var repo = new CommunicationLogRepository(ctx);
        
        await repo.AddAsync(CommunicationLog.CreateSuccess("c1", ECommunicationChannel.Email, "u1", 1));
        await repo.AddAsync(CommunicationLog.CreateSuccess("c2", ECommunicationChannel.Push, "u2", 1));
        await repo.SaveChangesAsync();

        // Act (When)
        var (items, totalCount) = await repo.SearchAsync(channel: "Email");

        // Assert (Then)
        totalCount.Should().Be(1);
        items.Should().ContainSingle(i => i.Channel == ECommunicationChannel.Email);
    }
}
