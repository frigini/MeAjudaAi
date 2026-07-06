using MeAjudaAi.Modules.Providers.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Database.Idempotency;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

[Collection("ProvidersIntegrationTests")]
public class ProviderIdempotencyIntegrationTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task InsertIfNotExistsAsync_ShouldInsert_WhenNewCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();

        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IIdempotencyRepository>();

        // Act
        await repository.MarkAsProcessedAsync(correlationId, CancellationToken.None);

        // Assert
        var isProcessed = await repository.IsProcessedAsync(correlationId, CancellationToken.None);
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
        await repository.MarkAsProcessedAsync(correlationId, CancellationToken.None);

        // Act - Second insert with same correlation_id (ON CONFLICT DO NOTHING)
        var act = async () => await repository.MarkAsProcessedAsync(correlationId, CancellationToken.None);

        // Assert - No exception thrown and still reported as processed
        await act.Should().NotThrowAsync();
        var isProcessed = await repository.IsProcessedAsync(correlationId, CancellationToken.None);
        isProcessed.Should().BeTrue();
    }
}
