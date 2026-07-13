using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class CorrelationHelperTests
{
    [Fact]
    public void ParseCorrelationId_WithValidCorrelationIdHeader_ReturnsHeaderGuid()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[AuthConstants.Headers.CorrelationId] = expected.ToString();

        // Act
        var result = CorrelationHelper.ParseCorrelationId(context);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ParseCorrelationId_WithInvalidCorrelationIdHeader_FallsBackToTraceIdentifier()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[AuthConstants.Headers.CorrelationId] = "not-a-guid";
        context.TraceIdentifier = expected.ToString();

        // Act
        var result = CorrelationHelper.ParseCorrelationId(context);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ParseCorrelationId_WithEmptyCorrelationIdHeader_FallsBackToTraceIdentifier()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[AuthConstants.Headers.CorrelationId] = "";
        context.TraceIdentifier = expected.ToString();

        // Act
        var result = CorrelationHelper.ParseCorrelationId(context);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ParseCorrelationId_WithNoHeaders_ReturnsNewGuid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "not-a-guid";

        // Act
        var result = CorrelationHelper.ParseCorrelationId(context);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ParseCorrelationId_WithEmptyTraceIdentifier_ReturnsNewGuid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "";

        // Act
        var result = CorrelationHelper.ParseCorrelationId(context);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ParseCorrelationId_CorrelationIdHeaderTakesPriorityOverTraceIdentifier()
    {
        // Arrange
        var headerGuid = Guid.NewGuid();
        var traceGuid = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[AuthConstants.Headers.CorrelationId] = headerGuid.ToString();
        context.TraceIdentifier = traceGuid.ToString();

        // Act
        var result = CorrelationHelper.ParseCorrelationId(context);

        // Assert
        result.Should().Be(headerGuid);
        result.Should().NotBe(traceGuid);
    }

    [Fact]
    public void ParseCorrelationId_WithWhitespaceCorrelationIdHeader_FallsBackToTraceIdentifier()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[AuthConstants.Headers.CorrelationId] = "   ";
        context.TraceIdentifier = expected.ToString();

        // Act
        var result = CorrelationHelper.ParseCorrelationId(context);

        // Assert
        result.Should().Be(expected);
    }
}
