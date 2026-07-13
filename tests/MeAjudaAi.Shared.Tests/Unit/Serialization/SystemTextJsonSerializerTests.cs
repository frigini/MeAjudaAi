using MeAjudaAi.Shared.Serialization;

namespace MeAjudaAi.Shared.Tests.Unit.Serialization;

public class SystemTextJsonSerializerTests
{
    private readonly SystemTextJsonSerializer _sut;

    public SystemTextJsonSerializerTests()
    {
        _sut = new SystemTextJsonSerializer(SerializationDefaults.Default);
    }


    [Fact]
    public void RoundTrip_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var obj = new TestObject { Id = Guid.NewGuid(), Name = "Test", Value = 123 };
        
        // Act
        var json = _sut.Serialize(obj);
        var result = _sut.Deserialize<TestObject>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(obj.Id);
        result.Name.Should().Be(obj.Name);
        result.Value.Should().Be(obj.Value);
    }

    private class TestObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
