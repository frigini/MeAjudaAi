using FluentAssertions;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

public class UuidGeneratorTests
{
    [Fact]
    public void NewId_ShouldReturnNonEmptyGuid()
    {
        // Act
        var id = UuidGenerator.NewId();

        // Assert
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewId_ShouldReturnUniqueValues()
    {
        // Act
        var id1 = UuidGenerator.NewId();
        var id2 = UuidGenerator.NewId();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void NewId_CalledMultipleTimes_ShouldReturnDifferentGuids()
    {
        // Arrange
        var ids = new HashSet<Guid>();
        var iterations = 1000;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            ids.Add(UuidGenerator.NewId());
        }

        // Assert
        ids.Should().HaveCount(iterations);
    }

    [Fact]
    public void NewIdString_ShouldReturnNonEmptyString()
    {
        // Act
        var id = UuidGenerator.NewIdString();

        // Assert
        id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NewIdString_ShouldReturnValidGuidFormat()
    {
        // Act
        var id = UuidGenerator.NewIdString();

        // Assert
        Guid.TryParse(id, out _).Should().BeTrue();
    }

    [Fact]
    public void NewIdString_ShouldContainHyphens()
    {
        // Act
        var id = UuidGenerator.NewIdString();

        // Assert
        id.Should().Contain("-");
        id.Length.Should().Be(36); // Formato GUID padrão: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
    }

    [Fact]
    public void NewIdString_ShouldReturnUniqueValues()
    {
        // Act
        var id1 = UuidGenerator.NewIdString();
        var id2 = UuidGenerator.NewIdString();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void NewIdString_ShouldBeVersion7()
    {
        // Act
        var id = UuidGenerator.NewIdString();

        // Assert
        id[14].Should().Be('7');
    }

    [Fact]
    public void NewIdStringCompact_ShouldReturnNonEmptyString()
    {
        // Act
        var id = UuidGenerator.NewIdStringCompact();

        // Assert
        id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NewIdStringCompact_ShouldNotContainHyphens()
    {
        // Act
        var id = UuidGenerator.NewIdStringCompact();

        // Assert
        id.Should().NotContain("-");
        id.Length.Should().Be(32); // Formato GUID compacto: 32 caracteres hexadecimais
    }

    [Fact]
    public void NewIdStringCompact_ShouldBeValidGuidWithoutHyphens()
    {
        // Act
        var id = UuidGenerator.NewIdStringCompact();

        // Assert
        Guid.TryParseExact(id, "N", out _).Should().BeTrue();
    }

    [Fact]
    public void NewIdStringCompact_ShouldReturnUniqueValues()
    {
        // Act
        var id1 = UuidGenerator.NewIdStringCompact();
        var id2 = UuidGenerator.NewIdStringCompact();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void NewIdStringCompact_ShouldBeVersion7()
    {
        // Act
        var id = UuidGenerator.NewIdStringCompact();

        // Assert
        id[12].Should().Be('7');
    }

    [Fact]
    public void IsValid_WithValidGuid_ShouldReturnTrue()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act
        var result = UuidGenerator.IsValid(validGuid);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithEmptyGuid_ShouldReturnFalse()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = UuidGenerator.IsValid(emptyGuid);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithGuidFromNewId_ShouldReturnTrue()
    {
        // Arrange
        var guid = UuidGenerator.NewId();

        // Act
        var result = UuidGenerator.IsValid(guid);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void IsValid_WithDefaultGuidString_ShouldReturnFalse(string guidString)
    {
        // Arrange
        var guid = Guid.Parse(guidString);

        // Act
        var result = UuidGenerator.IsValid(guid);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void NewIdString_AndNewIdStringCompact_ShouldRepresentSameConcept()
    {
        // Act
        var standardId = UuidGenerator.NewIdString();
        var compactId = UuidGenerator.NewIdStringCompact();

        // Faz parse de ambos para verificar que são GUIDs válidos
        var standardGuid = Guid.Parse(standardId);
        var compactGuid = Guid.ParseExact(compactId, "N");

        // Assert
        standardGuid.Should().NotBe(Guid.Empty);
        compactGuid.Should().NotBe(Guid.Empty);
    }
}
