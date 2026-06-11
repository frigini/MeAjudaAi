using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Services;

public class CommunicationsStartupValidatorTests
{
    private readonly Mock<ILogger<CommunicationsStartupValidator>> _loggerMock;

    public CommunicationsStartupValidatorTests()
    {
        _loggerMock = new Mock<ILogger<CommunicationsStartupValidator>>();
    }

    [Fact]
    public async Task StartAsync_WhenAllServicesRegistered_ShouldNotThrow()
    {
        // Arrange
        var emailSender = new Mock<IEmailSender>().Object;
        var smsSender = new Mock<ISmsSender>().Object;
        var pushSender = new Mock<IPushSender>().Object;

        var validator = new CommunicationsStartupValidator(
            stubsEnabled: true,
            emailSender,
            smsSender,
            pushSender,
            _loggerMock.Object);

        // Act & Assert
        await validator.StartAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WhenStubsEnabledAndNull_ShouldThrow()
    {
        // Arrange
        var validator = new CommunicationsStartupValidator(
            stubsEnabled: true,
            emailSender: null,
            smsSender: null,
            pushSender: null,
            _loggerMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(CancellationToken.None));
        
        ex.Message.Should().Contain("EnableStubs=true");
        ex.Message.Should().Contain("bug in service registration");
    }

    [Fact]
    public async Task StartAsync_WhenStubsDisabledAndMissing_ShouldThrow()
    {
        // Arrange
        var validator = new CommunicationsStartupValidator(
            stubsEnabled: false,
            emailSender: null,
            smsSender: null,
            pushSender: null,
            _loggerMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(CancellationToken.None));
        
        ex.Message.Should().Contain("EnableStubs=false");
        ex.Message.Should().Contain("real service providers");
    }

    [Fact]
    public async Task StartAsync_WhenStubsDisabledAndPartialMissing_ShouldThrowWithDetails()
    {
        // Arrange - Only email registered
        var emailSender = new Mock<IEmailSender>().Object;
        
        var validator = new CommunicationsStartupValidator(
            stubsEnabled: false,
            emailSender: emailSender,
            smsSender: null,
            pushSender: null,
            _loggerMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(CancellationToken.None));
        
        ex.Message.Should().Contain("ISmsSender");
        ex.Message.Should().Contain("IPushSender");
        ex.Message.Should().NotContain("IEmailSender");
    }

    [Fact]
    public async Task StopAsync_ShouldNotThrow()
    {
        // Arrange
        var validator = new CommunicationsStartupValidator(
            stubsEnabled: true,
            new Mock<IEmailSender>().Object,
            new Mock<ISmsSender>().Object,
            new Mock<IPushSender>().Object,
            _loggerMock.Object);

        // Act & Assert
        await validator.StopAsync(CancellationToken.None);
    }
}
