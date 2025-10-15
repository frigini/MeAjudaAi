using MeAjudaAi.Shared.Messaging.DeadLetter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.DeadLetter;

/// <summary>
/// Testes unitários para o serviço de Dead Letter Queue
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Shared")]
[Trait("Component", "DeadLetterService")]
public class DeadLetterServiceTests
{
    private readonly IDeadLetterService _deadLetterService;

    public DeadLetterServiceTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<IDeadLetterService, NoOpDeadLetterService>();

        var serviceProvider = services.BuildServiceProvider();
        _deadLetterService = serviceProvider.GetRequiredService<IDeadLetterService>();
    }

    [Theory]
    [InlineData(typeof(ArgumentException), 1, false, "Permanent exception should not retry")]
    [InlineData(typeof(ArgumentNullException), 1, false, "Permanent exception should not retry")]
    [InlineData(typeof(BusinessRuleException), 1, false, "Business rule exception should not retry")]
    [InlineData(typeof(TimeoutException), 1, true, "Transient exception should retry on first attempt")]
    [InlineData(typeof(TimeoutException), 5, false, "Transient exception should not retry after max attempts")]
    [InlineData(typeof(HttpRequestException), 2, true, "HTTP exception should retry")]
    [InlineData(typeof(OutOfMemoryException), 1, false, "Critical exception should not retry")]
    public void ShouldRetry_WithDifferentExceptionTypes_ReturnsExpectedResult(
        Type exceptionType, int attemptCount, bool expectedShouldRetry, string reason)
    {
        // Arrange
        var exception = CreateException(exceptionType);

        // Act
        var shouldRetry = _deadLetterService.ShouldRetry(exception, attemptCount);

        // Assert
        shouldRetry.Should().Be(expectedShouldRetry, reason);
    }

    [Theory]
    [InlineData(1, 2)] // Primeiro retry: 2^(1-1) * 2 = 2 segundos
    [InlineData(2, 4)] // Segundo retry: 2^(2-1) * 2 = 4 segundos  
    [InlineData(3, 8)] // Terceiro retry: 2^(3-1) * 2 = 8 segundos
    public void CalculateRetryDelay_WithDifferentAttempts_ReturnsExponentialBackoff(
        int attemptCount, int expectedSeconds)
    {
        // Act
        var delay = _deadLetterService.CalculateRetryDelay(attemptCount);

        // Assert
        delay.TotalSeconds.Should().BeApproximately(expectedSeconds, 0.1);
    }

    [Fact]
    public void CalculateRetryDelay_WithHighAttemptCount_DoesNotExceedMaxDelay()
    {
        // Arrange
        const int highAttemptCount = 10;

        // Act
        var delay = _deadLetterService.CalculateRetryDelay(highAttemptCount);

        // Assert
        delay.TotalSeconds.Should().BeLessOrEqualTo(300); // Máximo 5 minutos para NoOpDeadLetterService
    }

    [Fact]
    public async Task SendToDeadLetterAsync_WithValidMessage_CompletesSuccessfully()
    {
        // Arrange
        var message = new TestMessage { Id = "test-123", Content = "Test content" };
        var exception = new InvalidOperationException("Test exception");
        const string handlerType = "TestHandler";
        const string sourceQueue = "test-queue";
        const int attemptCount = 3;

        // Act & Assert
        var act = async () => await _deadLetterService.SendToDeadLetterAsync(
            message, exception, handlerType, sourceQueue, attemptCount);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListDeadLetterMessagesAsync_WithValidQueue_ReturnsEmptyList()
    {
        // Arrange
        const string queueName = "dlq.test-queue";

        // Act
        var messages = await _deadLetterService.ListDeadLetterMessagesAsync(queueName);

        // Assert
        messages.Should().NotBeNull();
        messages.Should().BeEmpty(); // NoOpDeadLetterService retorna lista vazia
    }

    [Fact]
    public async Task ReprocessDeadLetterMessageAsync_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        const string queueName = "dlq.test-queue";
        const string messageId = "test-message-123";

        // Act & Assert
        var act = async () => await _deadLetterService.ReprocessDeadLetterMessageAsync(queueName, messageId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PurgeDeadLetterMessageAsync_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        const string queueName = "dlq.test-queue";
        const string messageId = "test-message-123";

        // Act & Assert
        var act = async () => await _deadLetterService.PurgeDeadLetterMessageAsync(queueName, messageId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetDeadLetterStatisticsAsync_ReturnsEmptyStatistics()
    {
        // Act
        var statistics = await _deadLetterService.GetDeadLetterStatisticsAsync();

        // Assert
        statistics.Should().NotBeNull();
        statistics.TotalDeadLetterMessages.Should().Be(0);
        statistics.MessagesByQueue.Should().BeEmpty();
        statistics.MessagesByExceptionType.Should().BeEmpty();
    }

    private static Exception CreateException(Type exceptionType)
    {
        return exceptionType.Name switch
        {
            nameof(ArgumentException) => new ArgumentException("Test argument exception"),
            nameof(ArgumentNullException) => new ArgumentNullException("testParam", "Test null argument"),
            nameof(TimeoutException) => new TimeoutException("Test timeout"),
            nameof(HttpRequestException) => new HttpRequestException("Test HTTP exception"),
            nameof(InvalidOperationException) => new InvalidOperationException("Test invalid operation exception"),
            "BusinessRuleException" => new BusinessRuleException("TestRule", "Test business rule violation"),
            _ => new InvalidOperationException("Test exception")
        };
    }

    // Classe de mensagem de teste para testes
    private class TestMessage
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    // Mock de exceção de regra de negócio para testes
    private class BusinessRuleException : Exception
    {
        public string RuleName { get; }

        public BusinessRuleException(string ruleName, string message) : base(message)
        {
            RuleName = ruleName;
        }
    }
}
