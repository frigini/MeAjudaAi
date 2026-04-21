using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
public class PushSenderStubTests
{
    [Fact]
    public async Task SendAsync_ShouldReturnTrue_AndLog()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PushSenderStub>>();
        var stub = new PushSenderStub(loggerMock.Object);
        var notification = new PushNotification("token-123", "Title", "Body");

        // Act
        var result = await stub.SendAsync(notification);

        // Assert
        result.Should().BeTrue();
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Push notification sent")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
