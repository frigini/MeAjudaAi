using FluentAssertions;using FluentAssertions;

using MeAjudaAi.ServiceDefaults;using MeAjudaAi.Servic    [Fact]

using Microsoft.Extensions.DependencyInjection;    public void AddServiceDefaults_ShouldRegisterServices()

using Microsoft.Extensions.Configuration;    {

using Microsoft.Extensions.Hosting;        // Arrange

using Microsoft.Extensions.Logging;        var services = new ServiceCollection();

using Moq;        var mockBuilder = new Mock<IHostApplicationBuilder>();

using Xunit;        var mockConfigurationManager = new Mock<IConfigurationManager>();

        

namespace MeAjudaAi.ServiceDefaults.Tests.Unit;        mockBuilder.Setup(x => x.Services).Returns(services);

        mockBuilder.Setup(x => x.Configuration).Returns(mockConfigurationManager.Object);

public class ExtensionsTests

{        // Act

    [Fact]        Extensions.AddServiceDefaults(mockBuilder.Object);

    public void AddServiceDefaults_ShouldReturnBuilder()

    {        // Assert

        // Arrange        var serviceProvider = services.BuildServiceProvider();

        var services = new ServiceCollection();        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

        var mockBuilder = new Mock<IHostApplicationBuilder>();        loggerFactory.Should().NotBeNull();

        var mockConfigurationManager = new Mock<IConfigurationManager>();    }icrosoft.Extensions.DependencyInjection;

        using Microsoft.Extensions.Configuration;

        mockBuilder.Setup(x => x.Services).Returns(services);using Microsoft.Extensions.Hosting;

        mockBuilder.Setup(x => x.Configuration).Returns(mockConfigurationManager.Object);using Microsoft.Extensions.Logging;

using Moq;

        // Actusing Xunit;

        var result = Extensions.AddServiceDefaults(mockBuilder.Object);

namespace MeAjudaAi.ServiceDefaults.Tests.Unit;

        // Assert

        result.Should().NotBeNull();public class ExtensionsTests

        result.Should().BeSameAs(mockBuilder.Object);{

    }    private const string LocalhostTelemetry = "http://localhost:4317";

    [Fact]

    [Fact]    public void AddServiceDefaults_ShouldRegisterRequiredServices()

    public void AddServiceDefaults_WithNullBuilder_ShouldThrowArgumentNullException()    {

    {        // Arrange

        // Act & Assert        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => Extensions.AddServiceDefaults<IHostApplicationBuilder>(null!));        var configuration = new ConfigurationBuilder()

    }            .AddInMemoryCollection(new Dictionary<string, string?>

            {

    [Fact]                ["ConnectionStrings:DefaultConnection"] = "Data Source=test.db",

    public void AddServiceDefaults_ShouldRegisterServices()                ["Telemetry:Endpoint"] = LocalhostTelemetry

    {            })

        // Arrange            .Build();

        var services = new ServiceCollection();        

        var mockBuilder = new Mock<IHostApplicationBuilder>();        var mockBuilder = new Mock<IHostApplicationBuilder>();

        var mockConfigurationManager = new Mock<IConfigurationManager>();        var mockConfigurationManager = new Mock<IConfigurationManager>();

                mockBuilder.Setup(x => x.Services).Returns(services);

        mockBuilder.Setup(x => x.Services).Returns(services);        mockBuilder.Setup(x => x.Configuration).Returns(mockConfigurationManager.Object);

        mockBuilder.Setup(x => x.Configuration).Returns(mockConfigurationManager.Object);

        // Act

        // Act        var result = Extensions.AddServiceDefaults(mockBuilder.Object);

        Extensions.AddServiceDefaults(mockBuilder.Object);

        // Assert

        // Assert        result.Should().NotBeNull();

        var serviceProvider = services.BuildServiceProvider();        result.Should().BeSameAs(mockBuilder.Object);

        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();    }

        loggerFactory.Should().NotBeNull();

    }    [Fact]

    public void AddServiceDefaults_WithNullBuilder_ShouldThrowArgumentNullException()

    [Fact]    {

    public void AddServiceDefaults_ShouldAddLoggingServices()        // Act & Assert

    {        Assert.Throws<ArgumentNullException>(() => Extensions.AddServiceDefaults<IHostApplicationBuilder>(null!));

        // Arrange    }

        var services = new ServiceCollection();

        var mockBuilder = new Mock<IHostApplicationBuilder>();    [Fact]

        var mockConfigurationManager = new Mock<IConfigurationManager>();    public void AddServiceDefaults_ShouldConfigureLogging()

            {

        mockBuilder.Setup(x => x.Services).Returns(services);        // Arrange

        mockBuilder.Setup(x => x.Configuration).Returns(mockConfigurationManager.Object);        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder().Build();

        // Act        var mockBuilder = new Mock<IHostApplicationBuilder>();

        Extensions.AddServiceDefaults(mockBuilder.Object);        

        mockBuilder.Setup(x => x.Services).Returns(services);

        // Assert        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        services.Should().Contain(s => s.ServiceType == typeof(ILoggerFactory));

    }        // Act

}        Extensions.AddServiceDefaults(mockBuilder.Object);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        loggerFactory.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_ShouldAddLoggingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var mockBuilder = new Mock<IHostApplicationBuilder>();
        
        mockBuilder.Setup(x => x.Services).Returns(services);
        mockBuilder.Setup(x => x.Configuration).Returns(configuration);

        // Act
        Extensions.AddServiceDefaults(mockBuilder.Object);

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(ILoggerFactory));
    }
}
