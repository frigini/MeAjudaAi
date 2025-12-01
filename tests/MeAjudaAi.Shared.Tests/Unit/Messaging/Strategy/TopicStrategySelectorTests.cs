using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Events;
using MeAjudaAi.Modules.Users.Application.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Strategy;

namespace MeAjudaAi.Shared.Tests.UnitTests.Messaging.Strategy
{
    public class TopicStrategySelectorTests
    {
        private readonly ServiceBusOptions _options;
        private readonly TopicStrategySelector _selector;

        public TopicStrategySelectorTests()
        {
            _options = new ServiceBusOptions
            {
                DefaultTopicName = "default-topic",
                Strategy = ETopicStrategy.Hybrid,
                DomainTopics = new Dictionary<string, string>
                {
                    ["Users"] = "users-events",
                    ["Documents"] = "documents-events"
                }
            };

            _selector = new TopicStrategySelector(_options);
        }

        // Test events
        public record SimpleEvent(string Source) : IntegrationEvent(Source);

        public record UserCreatedEvent(string Source) : IntegrationEvent(Source);

        public record DocumentUploadedEvent(string Source) : IntegrationEvent(Source);

        [DedicatedTopic("dedicated-topic")]
        public record DedicatedTopicEvent(string Source) : IntegrationEvent(Source);

        [HighVolumeEvent]
        public record HighVolumeTestEvent(string Source) : IntegrationEvent(Source);

        [CriticalEvent]
        public record CriticalTestEvent(string Source) : IntegrationEvent(Source);

        [Fact]
        public void SelectTopicForEvent_WithDedicatedTopicAttribute_ShouldReturnDedicatedTopic()
        {
            // Act
            var result = _selector.SelectTopicForEvent<DedicatedTopicEvent>();

            // Assert
            result.Should().Be("dedicated-topic");
        }

        [Fact]
        public void SelectTopicForEvent_WithSingleWithFiltersStrategy_ShouldReturnDefaultTopic()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.SingleWithFilters;

            // Act
            var result = _selector.SelectTopicForEvent<SimpleEvent>();

            // Assert
            result.Should().Be("default-topic");
        }

        [Fact]
        public void SelectTopicForEvent_WithMultipleByDomainStrategy_ShouldReturnDomainTopic()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.MultipleByDomain;

            // Act
            var result = _selector.SelectTopicForEvent<UsersEvent>();

            // Assert
            result.Should().Be("users-events");
        }

        [Fact]
        public void SelectTopicForEvent_WithMultipleByDomainStrategyAndUnknownDomain_ShouldReturnDefaultTopic()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.MultipleByDomain;

            // Act
            var result = _selector.SelectTopicForEvent<SimpleEvent>();

            // Assert
            result.Should().Be("default-topic");
        }

        [Fact]
        public void SelectTopicForEvent_WithHybridStrategyAndHighVolumeEvent_ShouldReturnDomainTopic()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.Hybrid;

            // Act
            var result = _selector.SelectTopicForEvent<HighVolumeTestEvent>();

            // Assert
            result.Should().Be("default-topic"); // Shared domain maps to default
        }

        [Fact]
        public void SelectTopicForEvent_WithHybridStrategyAndCriticalEvent_ShouldReturnDomainTopic()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.Hybrid;

            // Act
            var result = _selector.SelectTopicForEvent<CriticalTestEvent>();

            // Assert
            result.Should().Be("default-topic"); // Shared domain maps to default
        }

        [Fact]
        public void SelectTopicForEvent_WithHybridStrategyAndNormalEvent_ShouldReturnDefaultTopic()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.Hybrid;

            // Act
            var result = _selector.SelectTopicForEvent<SimpleEvent>();

            // Assert
            result.Should().Be("default-topic");
        }

        [Fact]
        public void SelectTopicForEvent_WithTypeParameter_ShouldWorkSameAsGeneric()
        {
            // Act
            var genericResult = _selector.SelectTopicForEvent<SimpleEvent>();
            var typeResult = _selector.SelectTopicForEvent(typeof(SimpleEvent));

            // Assert
            typeResult.Should().Be(genericResult);
        }

        [Fact]
        public void SelectTopicForEvent_WithDedicatedTopicAttributeUsingTypeParameter_ShouldReturnDedicatedTopic()
        {
            // Act
            var result = _selector.SelectTopicForEvent(typeof(DedicatedTopicEvent));

            // Assert
            result.Should().Be("dedicated-topic");
        }

        [Fact]
        public void SelectTopicForEvent_ExtractsDomainFromNamespace()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.MultipleByDomain;

            // Act - namespace: MeAjudaAi.Modules.Documents.Application.Events
            var result = _selector.SelectTopicForEvent<DocumentsEvent>();

            // Assert
            result.Should().Be("documents-events");
        }

        [Fact]
        public void SelectTopicForEvent_WithShortNamespace_ShouldUseSharedDomain()
        {
            // Arrange
            _options.Strategy = ETopicStrategy.MultipleByDomain;

            // Act
            var result = _selector.SelectTopicForEvent<SimpleEvent>();

            // Assert
            result.Should().Be("default-topic"); // Shared domain not in dictionary
        }
    }
}

// Define test events in nested namespaces to test domain extraction
namespace MeAjudaAi.Modules.Users.Application.Events
{
    public record UsersEvent(string Source) : IntegrationEvent(Source);
}

namespace MeAjudaAi.Modules.Documents.Application.Events
{
    public record DocumentsEvent(string Source) : IntegrationEvent(Source);
}
