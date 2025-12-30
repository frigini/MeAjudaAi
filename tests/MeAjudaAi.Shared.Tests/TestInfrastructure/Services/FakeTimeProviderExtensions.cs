using Microsoft.Extensions.Time.Testing;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Services;

/// <summary>
/// Métodos de extensão para FakeTimeProvider para simplificar configuração de testes.
/// </summary>
public static class FakeTimeProviderExtensions
{
    /// <summary>
    /// Cria um FakeTimeProvider com data/hora UTC fixa.
    /// </summary>
    public static FakeTimeProvider CreateFixed(DateTime utcDateTime)
    {
        return new FakeTimeProvider(new DateTimeOffset(utcDateTime, TimeSpan.Zero));
    }

    /// <summary>
    /// Cria um FakeTimeProvider com a hora UTC atual.
    /// </summary>
    public static FakeTimeProvider CreateDefault()
    {
        return new FakeTimeProvider(DateTimeOffset.UtcNow);
    }
}
