using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Attributes;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Architecture.Tests.Helpers;
using System.Reflection;

namespace MeAjudaAi.Architecture.Tests;

public class EventCatalogTests
{
    private readonly IEnumerable<Assembly> _moduleAssemblies;

    public EventCatalogTests()
    {
        _moduleAssemblies = ModuleDiscoveryHelper.DiscoverModules()
            .SelectMany(m => new[] { m.DomainAssembly, m.ApplicationAssembly, m.InfrastructureAssembly })
            .Where(a => a != null)
            .Cast<Assembly>();
    }

    [Fact]
    public void CriticalEvents_ShouldHaveCriticalEventAttribute()
    {
        var criticalEvents = new[]
        {
            typeof(BookingCancelledIntegrationEvent),
            typeof(BookingCompletedIntegrationEvent),
            typeof(BookingConfirmedIntegrationEvent),
            typeof(BookingCreatedIntegrationEvent),
            typeof(BookingRejectedIntegrationEvent),
            typeof(SubscriptionActivatedIntegrationEvent),
            typeof(SubscriptionCanceledIntegrationEvent),
            typeof(SubscriptionExpiredIntegrationEvent),
            typeof(SubscriptionRenewedIntegrationEvent)
        };

        foreach (var eventType in criticalEvents)
        {
            eventType.Should().BeDecoratedWith<CriticalEventAttribute>(
                because: $"{eventType.Name} is a critical event and must have [CriticalEvent] attribute.");
        }
    }

    [Fact]
    public void AllActiveIntegrationEvents_ShouldHaveAtLeastOneHandler()
    {
        var integrationEvents = GetIntegrationEvents();
        var handlerTypes = GetIntegrationEventHandlers();

        foreach (var eventType in integrationEvents)
        {
            // Pular eventos no backlog ou sem handlers funcionais definidos na matriz como 'Pendente'
            if (eventType.Name.Contains("SearchableProviderIndexed") ||
                eventType.Name.Contains("AllowedCity"))
            {
                continue;
            }

            var hasHandler = handlerTypes.Any(h => h.GetInterfaces()
                .Any(i => i.IsGenericType && 
                          i.GetGenericTypeDefinition() == typeof(IEventHandler<>) && 
                          i.GetGenericArguments()[0] == eventType));

            hasHandler.Should().BeTrue(
                because: $"Integration event {eventType.Name} must have at least one registered handler.");
        }
    }

    private IEnumerable<Type> GetIntegrationEvents()
    {
        return _moduleAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IIntegrationEvent).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
    }

    private IEnumerable<Type> GetIntegrationEventHandlers()
    {
        return _moduleAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)));
    }
}
