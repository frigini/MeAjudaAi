using MeAjudaAi.Modules.Ratings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Database.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration.ContextQueries;

[Collection("RatingsIntegrationTests")]
public class RatingsIdempotencyIntegrationTests : RatingsIdempotencyTestBase
{
    [Fact]
    public async Task InsertIfNotExistsAsync_ShouldInsert_WhenNewCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IIdempotencyRepository>();

        // Act
        await repository.MarkAsProcessedAsync(correlationId);
        var isProcessed = await repository.IsProcessedAsync(correlationId);

        // Assert
        isProcessed.Should().BeTrue();
    }

    [Fact]
    public async Task InsertIfNotExistsAsync_ShouldReturnFalse_WhenDuplicateCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IIdempotencyRepository>();

        // Act - First insert
        await repository.MarkAsProcessedAsync(correlationId);

        // Act - Second insert (duplicate, should be idempotent)
        await repository.MarkAsProcessedAsync(correlationId);

        var isProcessed = await repository.IsProcessedAsync(correlationId);

        // Assert
        isProcessed.Should().BeTrue();
    }
}
