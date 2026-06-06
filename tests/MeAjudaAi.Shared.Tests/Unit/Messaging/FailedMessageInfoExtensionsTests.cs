using FluentAssertions;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using System.Net.Sockets;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

public class FailedMessageInfoExtensionsTests
{
    [Theory]
    [InlineData(typeof(ArgumentException), EFailureType.Permanent)]
    [InlineData(typeof(ArgumentNullException), EFailureType.Permanent)]
    [InlineData(typeof(FormatException), EFailureType.Permanent)]
    [InlineData(typeof(InvalidOperationException), EFailureType.Permanent)]
    [InlineData(typeof(TimeoutException), EFailureType.Transient)]
    [InlineData(typeof(HttpRequestException), EFailureType.Transient)]
    [InlineData(typeof(SocketException), EFailureType.Transient)]
    [InlineData(typeof(OutOfMemoryException), EFailureType.Critical)]
    [InlineData(typeof(StackOverflowException), EFailureType.Critical)]
    [InlineData(typeof(Exception), EFailureType.Unknown)]
    public void ClassifyFailure_ReturnsCorrectFailureType(Type exceptionType, EFailureType expectedType)
    {
        // Arrange
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        // Act
        var result = exception.ClassifyFailure();

        // Assert
        result.Should().Be(expectedType);
    }
}
