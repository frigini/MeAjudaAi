using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Services;

[Trait("Category", "Unit")]
public class SmsSenderStubTests
{
    [Fact]
    public async Task SendAsync_ShouldReturnTrue_AndLog()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SmsSenderStub>>();
        var stub = new SmsSenderStub(loggerMock.Object);
        var message = new SmsMessage("5511999999999", "Hello test");

        // Act
        var result = await stub.SendAsync(message);

        // Assert
        result.Should().BeTrue();
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SMS sent successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
