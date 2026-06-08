using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Modules.Bookings.Application.ModuleApi;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
[Trait("Module", "Bookings")]
[Trait("Layer", "Application")]
public class BookingsModuleApiTests
{
    private readonly Mock<IBookingQueries> _bookingQueriesMock;
    private readonly Mock<ILogger<BookingsModuleApi>> _loggerMock;
    private readonly BookingsModuleApi _api;

    public BookingsModuleApiTests()
    {
        _bookingQueriesMock = new Mock<IBookingQueries>();
        _loggerMock = new Mock<ILogger<BookingsModuleApi>>();
        _api = new BookingsModuleApi(_bookingQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryReturnsTrue_ShouldReturnSuccessTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryReturnsFalse_ShouldReturnSuccessFalse()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenQueryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _api.HasCompletedBookingAsync(clientId, providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Error checking booking history.");
    }

    [Fact]
    public async Task HasCompletedBookingAsync_WhenCancelled_ShouldPropagateException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var clientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        
        _bookingQueriesMock.Setup(q => q.HasCompletedBookingAsync(clientId, providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        Func<Task> act = () => _api.HasCompletedBookingAsync(clientId, providerId, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
