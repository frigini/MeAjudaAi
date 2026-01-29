using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Modules.Locations.Infrastructure.Filters;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.Filters;

public class LocationsExceptionHandlerTests
{
    private readonly Mock<ILogger<LocationsExceptionHandler>> _loggerMock;
    private readonly LocationsExceptionHandler _handler;

    public LocationsExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<LocationsExceptionHandler>>();
        _handler = new LocationsExceptionHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_NotFoundException_ShouldReturn404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var stream = new System.IO.MemoryStream();
        context.Response.Body = stream;
        
        var exception = new MeAjudaAi.Shared.Exceptions.NotFoundException("City", "123");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(404);
        
        // Verifying logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_DuplicateAllowedCityException_ShouldReturn409()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var stream = new System.IO.MemoryStream();
        context.Response.Body = stream;
        
        var exception = new DuplicateAllowedCityException("Muriaé", "MG");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task TryHandleAsync_InvalidLocationArgumentException_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var stream = new System.IO.MemoryStream();
        context.Response.Body = stream;
        
        var exception = new InvalidLocationArgumentException("Invalid argument");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400); // Mapped to BadRequest in code
    }

    private class TestBadRequestException(string message) : BadRequestException(message);

    [Fact]
    public async Task TryHandleAsync_BadRequestException_ShouldReturn400()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var stream = new System.IO.MemoryStream();
        context.Response.Body = stream;
        
        var exception = new TestBadRequestException("Bad request");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.Response.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task TryHandleAsync_UnknownException_ShouldReturnFalse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var exception = new Exception("Unknown error");

        // Act
        var result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        // Should not modify status code
        context.Response.StatusCode.Should().Be(200); // Default
    }
}
