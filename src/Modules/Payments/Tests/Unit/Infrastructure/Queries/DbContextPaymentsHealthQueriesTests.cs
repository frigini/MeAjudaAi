using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Infrastructure")]
public class DbContextPaymentsHealthQueriesTests : BaseInMemoryDatabaseTest<PaymentsDbContext>
{
    private readonly DbContextPaymentsHealthQueries _queries;

    public DbContextPaymentsHealthQueriesTests()
        : base(options => new PaymentsDbContext(options))
    {
        _queries = new DbContextPaymentsHealthQueries(DbContext);
    }

    [Fact]
    public async Task CanConnectAsync_ShouldReturnTrue_WhenDatabaseIsAvailable()
    {
        var result = await _queries.CanConnectAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanConnectAsync_ShouldCompleteSuccessfully_WhenValidCancellationToken()
    {
        var result = await _queries.CanConnectAsync(CancellationToken.None);

        result.Should().BeTrue();
    }
}
