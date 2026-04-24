using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.DeadLetter;

[Trait("Category", "Unit")]
public class RabbitMqDeadLetterServiceTests
{
    private readonly Mock<ILogger<RabbitMqDeadLetterService>> _loggerMock = new();
    private readonly RabbitMqOptions _rabbitMqOptions = new();
    private readonly DeadLetterOptions _deadLetterOptions = new();
    private readonly Mock<IOptions<DeadLetterOptions>> _optionsMock = new();
    private readonly Mock<IMessageSerializer> _serializerMock = new();

    public RabbitMqDeadLetterServiceTests()
    {
        _optionsMock.Setup(o => o.Value).Returns(_deadLetterOptions);
        _serializerMock.Setup(s => s.Serialize(It.IsAny<object>())).Returns("{}");
    }

    [Fact]
    public void ShouldRetry_WithTransientException_AndLowAttemptCount_ReturnsTrue()
    {
        // Arrange
        var service = new RabbitMqDeadLetterService(_rabbitMqOptions, _optionsMock.Object, _serializerMock.Object, _loggerMock.Object);
        var ex = new System.Net.Http.HttpRequestException("transient");
        _deadLetterOptions.MaxRetryAttempts = 3;

        // Act
        var result = service.ShouldRetry(ex, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_WithHighAttemptCount_ReturnsFalse()
    {
        // Arrange
        var service = new RabbitMqDeadLetterService(_rabbitMqOptions, _optionsMock.Object, _serializerMock.Object, _loggerMock.Object);
        var ex = new System.Net.Http.HttpRequestException("transient");
        _deadLetterOptions.MaxRetryAttempts = 3;

        // Act
        var result = service.ShouldRetry(ex, 3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateRetryDelay_ShouldFollowExponentialBackoff()
    {
        // Arrange
        var service = new RabbitMqDeadLetterService(_rabbitMqOptions, _optionsMock.Object, _serializerMock.Object, _loggerMock.Object);
        _deadLetterOptions.InitialRetryDelaySeconds = 2;
        _deadLetterOptions.BackoffMultiplier = 2.0;
        _deadLetterOptions.MaxRetryDelaySeconds = 30;

        // Act & Assert
        service.CalculateRetryDelay(1).TotalSeconds.Should().Be(2);
        service.CalculateRetryDelay(2).TotalSeconds.Should().Be(4);
        service.CalculateRetryDelay(3).TotalSeconds.Should().Be(8);
    }

    [Fact]
    public async Task SendToDeadLetterAsync_WhenConnectionFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _rabbitMqOptions.Host = "invalid-host";
        var service = new RabbitMqDeadLetterService(_rabbitMqOptions, _optionsMock.Object, _serializerMock.Object, _loggerMock.Object);
        var message = new { Id = 1 };
        var ex = new Exception("Original error");

        // Act
        var act = () => service.SendToDeadLetterAsync(message, ex, "Handler", "queue", 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
