using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Handlers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.DeadLetter;

/// <summary>
/// Testes unitários para o middleware de retry de mensagens
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
[Trait("Component", "MessageRetryMiddleware")]
public class MessageRetryMiddlewareTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MessageRetryMiddleware<TestMessage> _middleware;

    public MessageRetryMiddlewareTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        // Adiciona configuração
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Messaging:DeadLetter:Enabled"] = "true",
                ["Messaging:DeadLetter:MaxRetryAttempts"] = "3",
                ["Messaging:DeadLetter:InitialRetryDelaySeconds"] = "1",
                ["Messaging:DeadLetter:BackoffMultiplier"] = "2.0"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Adiciona mock do ambiente host
        services.AddSingleton<IHostEnvironment>(new MockHostEnvironment("Testing"));

        // Adiciona serviço de dead letter
        services.AddSingleton<IDeadLetterService, NoOpDeadLetterService>();

        _serviceProvider = services.BuildServiceProvider();

        var deadLetterService = _serviceProvider.GetRequiredService<IDeadLetterService>();
        var logger = _serviceProvider.GetRequiredService<ILogger<MessageRetryMiddleware<TestMessage>>>();

        _middleware = new MessageRetryMiddleware<TestMessage>(
            deadLetterService, logger, "TestHandler", "test-queue");
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithSuccessfulHandler_ReturnsTrue()
    {
        // Arrange
        var message = new TestMessage { Id = "test-123" };
        var callCount = 0;

        Task TestHandler(TestMessage msg, CancellationToken ct)
        {
            callCount++;
            return Task.CompletedTask;
        }

        // Act
        var result = await _middleware.ExecuteWithRetryAsync(message, TestHandler);

        // Assert
        result.Should().BeTrue();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithTransientFailureThenSuccess_ReturnsTrue()
    {
        // Arrange
        var message = new TestMessage { Id = "test-123" };
        var callCount = 0;

        Task TestHandler(TestMessage msg, CancellationToken ct)
        {
            callCount++;
            if (callCount < 3)
                throw new TimeoutException("Temporary failure");
            return Task.CompletedTask;
        }

        // Act
        var result = await _middleware.ExecuteWithRetryAsync(message, TestHandler);

        // Assert
        result.Should().BeTrue();
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithPermanentFailure_ReturnsFalse()
    {
        // Arrange
        var message = new TestMessage { Id = "test-123" };
        var callCount = 0;

        Task TestHandler(TestMessage msg, CancellationToken ct)
        {
            callCount++;
            throw new ArgumentException("Permanent failure");
        }

        // Act
        var result = await _middleware.ExecuteWithRetryAsync(message, TestHandler);

        // Assert
        result.Should().BeFalse(); // Enviado para DLQ
        callCount.Should().Be(1); // Nenhum retry para falhas permanentes
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithMaxRetriesExceeded_ReturnsFalse()
    {
        // Arrange
        var message = new TestMessage { Id = "test-123" };
        var callCount = 0;

        Task TestHandler(TestMessage msg, CancellationToken ct)
        {
            callCount++;
            throw new TimeoutException("Persistent failure");
        }

        // Act
        var result = await _middleware.ExecuteWithRetryAsync(message, TestHandler);

        // Assert
        result.Should().BeFalse(); // Enviado para DLQ após máximo de tentativas
        callCount.Should().BeGreaterThan(1); // Múltiplas tentativas realizadas
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithCancellation_ThrowsOperationCancelledException()
    {
        // Arrange
        var message = new TestMessage { Id = "test-123" };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Task TestHandler(TestMessage msg, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _middleware.ExecuteWithRetryAsync(message, TestHandler, cts.Token));
    }

    [Fact]
    public void MessageRetryMiddlewareFactory_CreateMiddleware_ReturnsValidInstance()
    {
        // Arrange
        var factory = new MessageRetryMiddlewareFactory(_serviceProvider);

        // Act
        var middleware = factory.CreateMiddleware<TestMessage>("TestHandler", "test-queue");

        // Assert
        middleware.Should().NotBeNull();
        middleware.Should().BeOfType<MessageRetryMiddleware<TestMessage>>();
    }

    [Fact]
    public async Task ExecuteWithRetryExtension_WithServiceProvider_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IDeadLetterService, NoOpDeadLetterService>();
        services.AddMessageRetryMiddleware();

        var serviceProvider = services.BuildServiceProvider();
        var message = new TestMessage { Id = "test-123" };
        var callCount = 0;

        Task TestHandler(TestMessage msg, CancellationToken ct)
        {
            callCount++;
            return Task.CompletedTask;
        }

        // Act
        var result = await message.ExecuteWithRetryAsync(TestHandler, serviceProvider, "test-queue");

        // Assert
        result.Should().BeTrue();
        callCount.Should().Be(1);
    }

    // Classe de mensagem de teste
    private class TestMessage
    {
        public string Id { get; set; } = string.Empty;
    }
}
