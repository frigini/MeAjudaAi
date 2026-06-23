using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Modules.Communications.Domain.Services;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
[Trait("Module", "Communications")]
[Trait("Layer", "Infrastructure")]
public class ExtensionsTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = DatabaseConstants.DefaultTestConnectionString,
                ["Communications:EnableStubs"] = "true"
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddLogging();
        services.AddSingleton(new Mock<MeAjudaAi.Shared.Messaging.IMessageBus>().Object);

        // Act
        MeAjudaAi.Modules.Communications.Infrastructure.Extensions.AddInfrastructure(services, configuration, environment.Object);

        // Assert - verify registrations exist in the collection
        services.Should().Contain(s => s.ServiceType == typeof(IEmailTemplateQueries));
        services.Should().Contain(s => s.ServiceType == typeof(ICommunicationLogQueries));
        services.Should().Contain(s => s.ServiceType == typeof(IOutboxMessageRepository));
        services.Should().Contain(s => s.ServiceType == typeof(IEmailSender));
        services.Should().Contain(s => s.ServiceType == typeof(ISmsSender));
        services.Should().Contain(s => s.ServiceType == typeof(IPushSender));
    }
}
