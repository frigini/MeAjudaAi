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
        var options = new MessageBusOptions();
        var sut = new OptionsTopicNameConvention(options);
        var eventType = typeof(TestEvent);

        // Act
        var result = sut.GetTopic(eventType);

        // Assert
        // Default convention should be "{lastPart}.events"
        result.Should().Be("conventions.events");
    }
}

public class TestEvent { }
