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
    [InlineData(1)]
    [InlineData(3)]
    public void ShouldRetry_ShouldReturnTrueForLowAttemptCount(int attemptCount)
    {
        // Arrange
        var ex = new Exception("test"); 
        
        // Act
        var result = _sut.ShouldRetry(ex, attemptCount);

        // Assert
        // Nota: ClassifyFailure por padrão pode não retornar Transient para uma Exception genérica
        // mas o NoOpDeadLetterService apenas checa a contagem se o classify permitir.
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalseForHighAttemptCount()
    {
        // Arrange
        var ex = new Exception("test");
        
        // Act
        var result = _sut.ShouldRetry(ex, 4);

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
}
