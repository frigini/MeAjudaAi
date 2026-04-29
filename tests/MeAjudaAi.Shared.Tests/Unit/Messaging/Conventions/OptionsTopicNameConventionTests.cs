using MeAjudaAi.Shared.Messaging.Options;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.Conventions;

public class OptionsTopicNameConventionTests
{
    [Fact]
    public void GetTopic_Should_DelegateToOptionsTopicNamingConvention()
    {
        // Arrange
        var customTopic = "custom-topic-for-test";
        var options = new MessageBusOptions
        {
            TopicNamingConvention = _ => customTopic
        };
        var sut = new OptionsTopicNameConvention(options);
        var eventType = typeof(TestEvent);

        // Act
        var result = sut.GetTopic(eventType);

        // Assert
        result.Should().Be(customTopic);
    }
}

public class TestEvent { }
