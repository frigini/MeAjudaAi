using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Modules.Documents.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
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
                ["ConnectionStrings:DefaultConnection"] = DatabaseConstants.DefaultTestConnectionString
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddLogging();
        services.AddSingleton(new Mock<MeAjudaAi.Shared.Messaging.IMessageBus>().Object);

        // Act
        MeAjudaAi.Modules.Documents.Infrastructure.Extensions.AddInfrastructure(services, configuration, environment.Object);

        // Assert - verify registrations exist in the collection
        services.Should().Contain(s => s.ServiceType == typeof(IDocumentQueries));
        services.Should().Contain(s => s.ServiceType == typeof(IBlobStorageService));
        services.Should().Contain(s => s.ServiceType == typeof(IDocumentIntelligenceService));
        services.Should().Contain(s => s.ServiceType == typeof(IDocumentVerificationService));

        // Assert - event handlers
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<DocumentVerifiedDomainEvent>));
    }
}
