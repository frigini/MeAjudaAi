using MeAjudaAi.Modules.Ratings.Infrastructure;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Infrastructure;

public class ExtensionsTests
{
    [Fact]
    public void AddEventHandlers_ShouldRegisterAllEventHandlers()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddSingleton<IConfiguration>(configuration); // Registra IConfiguration
        var hostEnv = new Mock<IHostEnvironment>();
        services.AddLogging();
        
        services.AddScoped(sp => new Mock<IUnitOfWork>().Object);
        // Mock do MessageBus para evitar falha de resolução
        services.AddSingleton(new Mock<IMessageBus>().Object);
        // Mock do CacheService para handlers que dependem dele
        services.AddSingleton(new Mock<ICacheService>().Object);

        Extensions.AddInfrastructure(services, configuration, hostEnv.Object);
        var provider = services.BuildServiceProvider();

        provider.GetService<IEventHandler<ReviewApprovedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ReviewRejectedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<UserDeletedIntegrationEvent>>().Should().NotBeNull();
    }
}
