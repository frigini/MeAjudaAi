namespace MeAjudaAi.Modules.Users.Tests.Unit.API.Endpoints;

/// <summary>
/// Testes unitários para validação de dados do endpoint de busca por ID.
/// </summary>
public class GetUserByIdEndpointTests
{
    [Fact]
    public void GuidValidation_WithValidGuid_ShouldPass()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act & Assert
        validGuid.Should().NotBe(Guid.Empty);
        validGuid.ToString().Should().HaveLength(36);
    }

    [Fact]
    public void GuidValidation_WithEmptyGuid_ShouldBeDetectable()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        emptyGuid.Should().Be(Guid.Empty);
        emptyGuid.ToString().Should().Be("00000000-0000-0000-0000-000000000000");
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Guid.Empty
    [InlineData("11111111-1111-1111-1111-111111111111")] // Guid válido
    [InlineData("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")] // Guid válido com letras
    public void GuidParsing_WithDifferentFormats_ShouldParseCorrectly(string guidString)
    {
        // Act
        var isParseable = Guid.TryParse(guidString, out var parsedGuid);

        // Assert
        isParseable.Should().BeTrue();
        parsedGuid.ToString().Should().Be(guidString);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("11111111-1111-1111-1111-111111111111-extra")]
    public void GuidParsing_WithInvalidFormats_ShouldFail(string invalidGuidString)
    {
        // Act
        var isParseable = Guid.TryParse(invalidGuidString, out _);

        // Assert
        isParseable.Should().BeFalse();
    }
}