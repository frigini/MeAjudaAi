using FluentAssertions;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "Infrastructure")]
public class DbContextCommunicationLogQueriesTests : IDisposable
{
    private readonly CommunicationsDbContext _db;
    private readonly DbContextCommunicationLogQueries _queries;

    public DbContextCommunicationLogQueriesTests()
    {
        _db = CommunicationsTestDb.CreateSqlite();
        _queries = new DbContextCommunicationLogQueries(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task SearchAsync_WithInvalidChannel_ShouldReturnEmptyResult()
    {
        var log = CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "test@test.com", 1);
        _db.CommunicationLogs.Add(log);
        await _db.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(channel: "InvalidChannel");

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithValidFilters_ShouldReturnMatchingLogs()
    {
        var log = CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "test@test.com", 1);
        _db.CommunicationLogs.Add(log);
        await _db.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(correlationId: "corr-1", recipient: "test@test.com", isSuccess: true);

        items.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetByRecipientAsync_ShouldClampMaxResults()
    {
        var log = CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "test@test.com", 1);
        _db.CommunicationLogs.Add(log);
        await _db.SaveChangesAsync();

        // Testing that maxResults <= 0 is handled (clamped to 1 by Math.Clamp(maxResults, 1, 100))
        var items = await _queries.GetByRecipientAsync("test@test.com", maxResults: 0);

        items.Should().HaveCount(1);
    }
}
