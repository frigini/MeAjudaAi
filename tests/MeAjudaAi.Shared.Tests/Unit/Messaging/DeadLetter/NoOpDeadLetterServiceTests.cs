using FluentAssertions;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.DeadLetter;

[Trait("Category", "Unit")]
public class NoOpDeadLetterServiceTests
{
    private readonly Mock<ILogger<NoOpDeadLetterService>> _loggerMock = new();
    private readonly NoOpDeadLetterService _sut;

    public NoOpDeadLetterServiceTests()
    {
        _sut = new NoOpDeadLetterService(_loggerMock.Object);
    }

    [Fact]
    public async Task SendToDeadLetterAsync_ShouldCompleteSuccessfully()
    {
        // Act
        var act = () => _sut.SendToDeadLetterAsync(new { }, new Exception("error"), "handler", "queue", 1);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void ShouldRetry_WithTransientException_ShouldReturnExpectedResult(int attemptCount, bool expected)
    {
        // Arrange
        var ex = new System.Net.Http.HttpRequestException("transient");
        
        // Act
        var result = _sut.ShouldRetry(ex, attemptCount);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ShouldRetry_WithPermanentException_ShouldReturnFalse()
    {
        // Arrange
        var ex = new ArgumentException("permanent");
        
        // Act
        var result = _sut.ShouldRetry(ex, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateRetryDelay_ShouldReturnCorrectDelay()
    {
        // Act & Assert
        _sut.CalculateRetryDelay(1).TotalSeconds.Should().Be(2); // 2^(1-1) * 2 = 2
        _sut.CalculateRetryDelay(2).TotalSeconds.Should().Be(4); // 2^(2-1) * 2 = 4
        _sut.CalculateRetryDelay(10).TotalSeconds.Should().Be(300); // Max delay
    }

    [Fact]
    public async Task GetDeadLetterStatisticsAsync_ShouldReturnNotNull()
    {
        // Act
        var result = await _sut.GetDeadLetterStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ReprocessDeadLetterMessageAsync_ShouldCompleteSuccessfully()
    {
        // Act
        var act = () => _sut.ReprocessDeadLetterMessageAsync("queue", "msg-id");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ListDeadLetterMessagesAsync_ShouldReturnEmptyList()
    {
        // Act
        var result = await _sut.ListDeadLetterMessagesAsync("queue");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task PurgeDeadLetterMessageAsync_ShouldCompleteSuccessfully()
    {
        // Act
        var act = () => _sut.PurgeDeadLetterMessageAsync("queue", "msg-id");

        // Assert
        await act.Should().NotThrowAsync();
    }
}
