using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "Infrastructure")]
public class DbContextCommunicationLogQueriesTests : BaseInMemoryDatabaseTest<CommunicationsDbContext>
{
    private readonly DbContextCommunicationLogQueries _queries;

    public DbContextCommunicationLogQueriesTests() : base(options => new CommunicationsDbContext(options))
    {
        _queries = new DbContextCommunicationLogQueries(DbContext);
    }

    [Fact]
    public async Task SearchAsync_WithInvalidChannel_ShouldReturnEmptyResult()
    {
        var log = CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "test@test.com", 1);
        DbContext.CommunicationLogs.Add(log);
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery(Channel: "InvalidChannel"));

        items.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithValidFilters_ShouldReturnMatchingLogs()
    {
        var log = CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "test@test.com", 1);
        DbContext.CommunicationLogs.Add(log);
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery(CorrelationId: "corr-1", Recipient: "test@test.com", IsSuccess: true));

        items.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetByRecipientAsync_ShouldClampMaxResults()
    {
        var log = CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "test@test.com", 1);
        DbContext.CommunicationLogs.Add(log);
        await DbContext.SaveChangesAsync();

        // Testing that maxResults <= 0 is handled (clamped to 1 by Math.Clamp(maxResults, 1, 100))
        var items = await _queries.GetByRecipientAsync("test@test.com", maxResults: 0);

        items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WhenExists_ShouldReturnTrue()
    {
        var log = CommunicationLog.CreateSuccess("corr-exists", ECommunicationChannel.Email, "test@test.com", 1);
        DbContext.CommunicationLogs.Add(log);
        await DbContext.SaveChangesAsync();

        var result = await _queries.ExistsByCorrelationIdAsync("corr-exists");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByCorrelationIdAsync_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _queries.ExistsByCorrelationIdAsync("nonexistent");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByRecipientAsync_WhenNoResults_ShouldReturnEmpty()
    {
        var items = await _queries.GetByRecipientAsync("nobody@test.com");

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ShouldReturnAll()
    {
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "a@test.com", 1));
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-2", ECommunicationChannel.Sms, "b@test.com", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery());

        total.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_WithIsSuccessFilter_ShouldReturnMatching()
    {
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "a@test.com", 1));
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateFailure("corr-2", ECommunicationChannel.Email, "b@test.com", "error", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery(IsSuccess: true));

        total.Should().Be(1);
        items[0].CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public async Task SearchAsync_WithValidChannel_ShouldReturnOnlyMatchingChannel()
    {
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-email", ECommunicationChannel.Email, "a@test.com", 1));
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-sms", ECommunicationChannel.Sms, "b@test.com", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery(Channel: "Email"));

        total.Should().Be(1);
        items[0].CorrelationId.Should().Be("corr-email");
    }

    [Fact]
    public async Task SearchAsync_WithRecipientFilter_ShouldReturnOnlyMatchingRecipient()
    {
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "alice@test.com", 1));
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-2", ECommunicationChannel.Email, "bob@test.com", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery(Recipient: "alice"));

        total.Should().Be(1);
        items[0].CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public async Task SearchAsync_WithCorrelationIdFilter_ShouldReturnOnlyMatchingCorrelation()
    {
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("unique-corr", ECommunicationChannel.Email, "a@test.com", 1));
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("other-corr", ECommunicationChannel.Email, "b@test.com", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery(CorrelationId: "unique"));

        total.Should().Be(1);
        items[0].CorrelationId.Should().Be("unique-corr");
    }

    [Fact]
    public async Task SearchAsync_WithIsSuccessFalse_ShouldReturnOnlyFailures()
    {
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "a@test.com", 1));
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateFailure("corr-2", ECommunicationChannel.Email, "b@test.com", "error", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery(IsSuccess: false));

        total.Should().Be(1);
        items[0].CorrelationId.Should().Be("corr-2");
    }

    [Fact]
    public async Task SearchAsync_WithPageNumberBelowOne_ShouldClampToFirstPage()
    {
        DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess("corr-1", ECommunicationChannel.Email, "a@test.com", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery { PageNumber = 0, PageSize = 10 });

        total.Should().Be(1);
        items.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_WithPageSizeAboveLimit_ShouldClampTo100()
    {
        for (int i = 0; i < 5; i++)
            DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess($"corr-{i}", ECommunicationChannel.Email, "a@test.com", 1));
        await DbContext.SaveChangesAsync();

        var (items, total) = await _queries.SearchAsync(new CommunicationLogQuery { PageNumber = 1, PageSize = 200 });

        total.Should().Be(5);
        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetByRecipientAsync_WhenMaxResultsAboveLimit_ShouldClampTo100()
    {
        for (int i = 0; i < 3; i++)
            DbContext.CommunicationLogs.Add(CommunicationLog.CreateSuccess($"corr-{i}", ECommunicationChannel.Email, "a@test.com", 1));
        await DbContext.SaveChangesAsync();

        var items = await _queries.GetByRecipientAsync("a@test.com", maxResults: 200);

        items.Should().HaveCount(3);
    }
}
