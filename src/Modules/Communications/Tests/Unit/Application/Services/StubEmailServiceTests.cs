using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Modules.Communications.Application.Services.Email;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Services;

public class StubEmailServiceTests
{
    private readonly Mock<ILogger<StubEmailService>> _loggerMock;
    private readonly StubEmailService _service;

    public StubEmailServiceTests()
    {
        _loggerMock = new Mock<ILogger<StubEmailService>>();
        _service = new StubEmailService(_loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_ShouldReturnSuccess()
    {
        // Arrange
        var dto = new EmailMessageDto("test@test.com", "Subject", "Body");

        // Act
        var result = await _service.SendAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("stub_");
    }
}
