using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Modules.Communications.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure.Services;

public class CommunicationsStartupValidatorTests
{
    private readonly Mock<ILogger<CommunicationsStartupValidator>> _loggerMock;

    public CommunicationsStartupValidatorTests()
    {
        _loggerMock = new Mock<ILogger<CommunicationsStartupValidator>>();
    }

    private static IServiceProvider BuildServiceProvider(
        IEmailSender? emailSender = null,
        ISmsSender? smsSender = null,
        IPushSender? pushSender = null)
    {
        var services = new ServiceCollection();
        if (emailSender is not null) services.AddSingleton(emailSender);
        if (smsSender is not null) services.AddSingleton(smsSender);
        if (pushSender is not null) services.AddSingleton(pushSender);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task StartAsync_WhenAllServicesRegistered_ShouldNotThrow()
    {
        var sp = BuildServiceProvider(
            new Mock<IEmailSender>().Object,
            new Mock<ISmsSender>().Object,
            new Mock<IPushSender>().Object);

        var validator = new CommunicationsStartupValidator(
            stubsEnabled: true, sp, _loggerMock.Object);

        await validator.StartAsync(CancellationToken.None);
    }

    [Fact]
    public async Task StartAsync_WhenStubsEnabledAndNull_ShouldThrow()
    {
        var sp = BuildServiceProvider();

        var validator = new CommunicationsStartupValidator(
            stubsEnabled: true, sp, _loggerMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(CancellationToken.None));

        ex.Message.Should().Contain("EnableStubs=true");
        ex.Message.Should().Contain("bug in service registration");
    }

    [Fact]
    public async Task StartAsync_WhenStubsDisabledAndMissing_ShouldThrow()
    {
        var sp = BuildServiceProvider();

        var validator = new CommunicationsStartupValidator(
            stubsEnabled: false, sp, _loggerMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(CancellationToken.None));

        ex.Message.Should().Contain("EnableStubs=false");
        ex.Message.Should().Contain("real service providers");
    }

    [Fact]
    public async Task StartAsync_WhenStubsDisabledAndPartialMissing_ShouldThrowWithDetails()
    {
        var sp = BuildServiceProvider(emailSender: new Mock<IEmailSender>().Object);

        var validator = new CommunicationsStartupValidator(
            stubsEnabled: false, sp, _loggerMock.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(CancellationToken.None));

        ex.Message.Should().Contain("ISmsSender");
        ex.Message.Should().Contain("IPushSender");
        ex.Message.Should().NotContain("IEmailSender");
    }

    [Fact]
    public async Task StopAsync_ShouldNotThrow()
    {
        var sp = BuildServiceProvider(
            new Mock<IEmailSender>().Object,
            new Mock<ISmsSender>().Object,
            new Mock<IPushSender>().Object);

        var validator = new CommunicationsStartupValidator(
            stubsEnabled: true, sp, _loggerMock.Object);

        await validator.StopAsync(CancellationToken.None);
    }
}
