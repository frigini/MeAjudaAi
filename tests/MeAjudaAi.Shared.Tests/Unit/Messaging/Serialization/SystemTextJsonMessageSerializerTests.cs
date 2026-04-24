using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Serialization;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Messaging.Serialization;

[Trait("Category", "Unit")]
public class SystemTextJsonMessageSerializerTests
{
    private readonly SystemTextJsonMessageSerializer _sut = new();

    [Fact]
    public void Serialize_WithPrimitiveTypes_ShouldReturnValidJson()
    {
        // Arrange
        var data = new { Id = 1, Name = "Test", IsActive = true };

        // Act
        var json = _sut.Serialize(data);

        // Assert
        json.Should().Contain("\"id\":1");
        json.Should().Contain("\"name\":\"Test\"");
        json.Should().Contain("\"isActive\":true");
    }

    [Fact]
    public void RoundTrip_WithComplexObject_ShouldPreserveData()
    {
        // Arrange
        var original = new TestMessage
        {
            Id = Guid.NewGuid(),
            Amount = 150.50m,
            Tags = ["tag1", "tag2"],
            Metadata = new Dictionary<string, string> { ["key"] = "value" }
        };

        // Act
        var json = _sut.Serialize(original);
        var deserialized = _sut.Deserialize<TestMessage>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(original.Id);
        deserialized.Amount.Should().Be(original.Amount);
        deserialized.Tags.Should().BeEquivalentTo(original.Tags);
        deserialized.Metadata.Should().BeEquivalentTo(original.Metadata);
    }

    [Fact]
    public void Deserialize_DictionaryWithObjectValues_ShouldHaveJsonElementValues()
    {
        // Arrange
        var original = new Dictionary<string, object>
        {
            ["string"] = "value",
            ["number"] = 123,
            ["bool"] = true,
            ["nested"] = new { Foo = "bar" }
        };
        var json = _sut.Serialize(original);

        // Act
        var deserialized = _sut.Deserialize<Dictionary<string, object>>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!["string"].Should().BeOfType<JsonElement>();
        ((JsonElement)deserialized["string"]).GetString().Should().Be("value");
        ((JsonElement)deserialized["number"]).GetInt32().Should().Be(123);
        ((JsonElement)deserialized["bool"]).GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void FailedMessageInfo_RoundTrip_ShouldPreserveOriginalMessageAsString()
    {
        // Arrange
        var originalMessageJson = "{\"foo\":\"bar\"}";
        var failedMessage = new FailedMessageInfo
        {
            MessageId = "msg-123",
            OriginalMessage = originalMessageJson,
            MessageHeaders = new Dictionary<string, object> { ["trace-id"] = "trace-456" }
        };

        // Act
        var json = _sut.Serialize(failedMessage);
        var deserialized = _sut.Deserialize<FailedMessageInfo>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.MessageId.Should().Be("msg-123");
        deserialized.OriginalMessage.Should().Be(originalMessageJson);
        deserialized.MessageHeaders["trace-id"].Should().BeOfType<JsonElement>();
    }

    private class TestMessage
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public List<string> Tags { get; set; } = [];
        public Dictionary<string, string> Metadata { get; set; } = [];
    }
}
