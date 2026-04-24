using FluentAssertions;
using MeAjudaAi.Shared.Messaging.Attributes;
using MeAjudaAi.Shared.Messaging.Rebus.Conventions;
using Moq;
using Rebus.Topic;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.Conventions;

[Trait("Category", "Unit")]
public class AttributeTopicNameConventionTests
{
    private readonly Mock<ITopicNameConvention> _fallbackMock;
    private readonly AttributeTopicNameConvention _sut;

    public AttributeTopicNameConventionTests()
    {
        _fallbackMock = new Mock<ITopicNameConvention>();
        _sut = new AttributeTopicNameConvention(_fallbackMock.Object);
    }

    [Fact]
    public void GetTopic_Should_ReturnAttributeValue_When_AttributeIsPresent()
    {
        // Act
        var result = _sut.GetTopic(typeof(AttributedMessage));

        // Assert
        result.Should().Be("custom-topic");
        _fallbackMock.Verify(m => m.GetTopic(It.IsAny<Type>()), Times.Never);
    }

    [Fact]
    public void GetTopic_Should_ReturnFallbackValue_When_AttributeIsMissing()
    {
        // Arrange
        _fallbackMock.Setup(m => m.GetTopic(typeof(NonAttributedMessage)))
            .Returns("fallback-topic");

        // Act
        var result = _sut.GetTopic(typeof(NonAttributedMessage));

        // Assert
        result.Should().Be("fallback-topic");
        _fallbackMock.Verify(m => m.GetTopic(typeof(NonAttributedMessage)), Times.Once);
    }

    [DedicatedTopic("custom-topic")]
    private class AttributedMessage;

    private class NonAttributedMessage;
}
