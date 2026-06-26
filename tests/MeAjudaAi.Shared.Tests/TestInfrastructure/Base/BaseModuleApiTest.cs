using FluentAssertions;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

public abstract class BaseModuleApiTest
{
    protected async Task AssertIsAvailableReturnsTrue(
        Func<CancellationToken, Task<bool>> isAvailableAsync)
    {
        var result = await isAvailableAsync(default);
        result.Should().BeTrue();
    }

    protected async Task AssertIsAvailableReturnsFalse(
        Func<CancellationToken, Task<bool>> isAvailableAsync)
    {
        var result = await isAvailableAsync(default);
        result.Should().BeFalse();
    }

    protected async Task AssertIsAvailableThrows_WhenCancelled(
        Func<CancellationToken, Task<bool>> isAvailableAsync)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            isAvailableAsync(cts.Token));
    }
}
