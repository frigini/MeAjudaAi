using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Factories;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.Rebus;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging;

public class MessageBusFactoryTests
{
    private readonly Mock<IHostEnvironment> _envMock = new();
    private readonly Mock<IServiceProvider> _spMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogger<MessageBusFactory>> _loggerMock = new();

    [Fact]
    public void CreateMessageBus_ShouldReturnNoOp_WhenEnvironmentIsTesting()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(EnvironmentNames.Testing);
        _configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(new Mock<IConfigurationSection>().Object);
        
        var noOp = new NoOpMessageBus(new Mock<ILogger<NoOpMessageBus>>().Object);
        _spMock.Setup(s => s.GetService(typeof(NoOpMessageBus))).Returns(noOp);

        var factory = new MessageBusFactory(_envMock.Object, _spMock.Object, _configMock.Object, _loggerMock.Object);

        // Act
        var bus = factory.CreateMessageBus();

        // Assert
        bus.Should().BeOfType<NoOpMessageBus>();
    }

    [Fact]
    public void CreateMessageBus_ShouldReturnNoOp_WhenMessagingIsDisabled()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(EnvironmentNames.Development);
        
        // Mock configuration Messaging:Enabled = false
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(s => s.Value).Returns("false");
        _configMock.Setup(c => c.GetSection("Messaging:Enabled")).Returns(configSectionMock.Object);

        var noOp = new NoOpMessageBus(new Mock<ILogger<NoOpMessageBus>>().Object);
        _spMock.Setup(s => s.GetService(typeof(NoOpMessageBus))).Returns(noOp);

        var factory = new MessageBusFactory(_envMock.Object, _spMock.Object, _configMock.Object, _loggerMock.Object);

        // Act
        var bus = factory.CreateMessageBus();

        // Assert
        bus.Should().BeOfType<NoOpMessageBus>();
    }

    [Fact]
    public void CreateMessageBus_ShouldThrow_WhenRebusResolutionFails()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns(EnvironmentNames.Development);
        
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(s => s.Value).Returns("true");
        _configMock.Setup(c => c.GetSection("Messaging:Enabled")).Returns(configSectionMock.Object);

        _spMock.Setup(s => s.GetService(typeof(RebusMessageBus))).Throws(new InvalidOperationException("Dependency missing"));

        var factory = new MessageBusFactory(_envMock.Object, _spMock.Object, _configMock.Object, _loggerMock.Object);

        // Act
        var act = () => factory.CreateMessageBus();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Failed to initialize Rebus MessageBus*");
    }
}
