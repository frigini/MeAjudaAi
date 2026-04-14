using MeAjudaAi.Modules.Ratings.Application.Services;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Application.Services;

public class ContentModeratorTests
{
    private readonly ContentModerator _moderator;

    public ContentModeratorTests()
    {
        _moderator = new ContentModerator();
    }

    [Theory]
    [InlineData("Esse serviço é muito bom")]
    [InlineData("Gostei bastante")]
    [InlineData(null)]
    [InlineData("")]
    public void IsClean_WithCleanContent_ShouldReturnTrue(string? content)
    {
        // Act
        var result = _moderator.IsClean(content);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Você é um idiota")]
    [InlineData("Isso é um golpe")]
    [InlineData("Que lixo de serviço")]
    public void IsClean_WithDirtyContent_ShouldReturnFalse(string content)
    {
        // Act
        var result = _moderator.IsClean(content);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsClean_WithPartialMatch_ShouldReturnTrue()
    {
        // Bad word is "burro", but "burro" inside another word like "sussurro" should be clean
        // Act
        var result = _moderator.IsClean("O sussurro do vento");

        // Assert
        result.Should().BeTrue();
    }
}
