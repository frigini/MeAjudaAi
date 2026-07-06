using MeAjudaAi.Modules.Ratings.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Database.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration;

[Collection("RatingsIntegrationTests")]
public class RatingsIdempotencyIntegrationTests : RatingsIntegrationTestBase
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
    public async Task MarkAsProcessedAsync_ShouldBeIdempotent_WhenCalledTwiceWithSameCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IIdempotencyRepository>();

        // Act - First insert
        await repository.MarkAsProcessedAsync(correlationId);

        // Act - Second insert (duplicate, should be idempotent via ON CONFLICT DO NOTHING)
        var act = async () => await repository.MarkAsProcessedAsync(correlationId);

        // Assert - No exception and state remains processed
        await act.Should().NotThrowAsync();
        var isProcessed = await repository.IsProcessedAsync(correlationId);
        isProcessed.Should().BeTrue();
    }
}
