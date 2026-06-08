using MeAjudaAi.Modules.Ratings.Domain.Events;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

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
        
        services.AddScoped(sp => new Mock<MeAjudaAi.Shared.Database.Abstractions.IUnitOfWork>().Object);
        // Mock do MessageBus para evitar falha de resolução
        services.AddSingleton(new Mock<MeAjudaAi.Shared.Messaging.IMessageBus>().Object);

        MeAjudaAi.Modules.Ratings.Infrastructure.Extensions.AddInfrastructure(services, configuration, hostEnv.Object);
        var provider = services.BuildServiceProvider();

        provider.GetService<IEventHandler<ReviewApprovedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ReviewRejectedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<UserDeletedIntegrationEvent>>().Should().NotBeNull();
    }
}
