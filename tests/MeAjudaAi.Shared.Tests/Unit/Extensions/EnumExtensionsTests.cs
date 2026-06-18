using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

// Dummy enum for testing
public enum TestStatus
{
    Active,
    Inactive,
    Pending
}

// Enum dummy com atributos Display para testar ToDescription/ToDisplayName
public enum TestStatusWithAttributes
{
    [Display(Name = "Ativo")]
    Active,

    [Display(Name = "Inativo")]
    Inactive,

    [Display(Name = "Pendente")]
    Pending
}

// Enum dummy com atributos Description para testar fallback do ToDescription
public enum TestStatusWithDescription
{
    [Description("Descrição do Ativo")]
    Active,

    [Description("Descrição do Inativo")]
    Inactive,

    [Description("Descrição do Pendente")]
    Pending
}

[Trait("Category", "Unit")]
public class EnumExtensionsTests
{
    [Theory]
    [InlineData("Active", TestStatus.Active)]
    [InlineData("active", TestStatus.Active)] // Default ignoreCase = true
    [InlineData("Pending", TestStatus.Pending)]
    public void ToEnum_WithValidValue_ShouldReturnSuccess(string input, TestStatus expected)
    {
        // Act
        var result = input.ToEnum<TestStatus>();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void ToEnum_WithCaseSensitivity_ShouldReturnFailure()
    {
        // Act
        var result = "active".ToEnum<TestStatus>(ignoreCase: false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(400);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("InvalidValue")]
    public void ToEnum_WithInvalidValue_ShouldReturnFailure(string? input)
    {
        // Act
        var result = input!.ToEnum<TestStatus>();

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData("Active", TestStatus.Pending, TestStatus.Active)]
    [InlineData("active", TestStatus.Pending, TestStatus.Active)]
    [InlineData("Invalid", TestStatus.Pending, TestStatus.Pending)]
    [InlineData(null, TestStatus.Pending, TestStatus.Pending)]
    [InlineData("", TestStatus.Pending, TestStatus.Pending)]
    public void ToEnumOrDefault_ShouldReturnExpectedValue(string? input, TestStatus defaultValue, TestStatus expected)
    {
        // Act
        var result = input!.ToEnumOrDefault(defaultValue);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Active", true)]
    [InlineData("active", true)]
    [InlineData("Invalid", false)]
    [InlineData(null, false)]
    public void IsValidEnum_ShouldReturnExpectedResult(string? input, bool expected)
    {
        // Act
        var result = input!.IsValidEnum<TestStatus>();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetValidValues_ShouldReturnAllNames()
    {
        // Act
        var result = EnumExtensions.GetValidValues<TestStatus>();

        // Assert
        result.Should().Contain(new[] { "Active", "Inactive", "Pending" });
        result.Length.Should().Be(3);
    }

    [Fact]
    public void GetValidValuesDescription_ShouldReturnFormattedString()
    {
        // Act
        var result = EnumExtensions.GetValidValuesDescription<TestStatus>();

        // Assert
        result.Should().Contain("Active, Inactive, Pending");
        result.Should().StartWith("Valores válidos para TestStatus:");
    }

    [Theory]
    [InlineData(TestStatusWithAttributes.Active, "Ativo")]
    [InlineData(TestStatusWithAttributes.Inactive, "Inativo")]
    [InlineData(TestStatusWithAttributes.Pending, "Pendente")]
    public void ToDescription_WithDisplayAttribute_ShouldReturnDisplayName(TestStatusWithAttributes input, string expected)
    {
        // Act
        var result = input.ToDescription();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(TestStatus.Active, "Active")]
    [InlineData(TestStatus.Inactive, "Inactive")]
    [InlineData(TestStatus.Pending, "Pending")]
    public void ToDescription_WithoutDisplayAttribute_ShouldReturnEnumName(TestStatus input, string expected)
    {
        // Act
        var result = input.ToDescription();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(TestStatusWithAttributes.Active, "Ativo")]
    [InlineData(TestStatusWithAttributes.Inactive, "Inativo")]
    [InlineData(TestStatusWithAttributes.Pending, "Pendente")]
    public void ToDisplayName_WithDisplayAttribute_ShouldReturnDisplayName(TestStatusWithAttributes input, string expected)
    {
        // Act
        var result = input.ToDisplayName();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(TestStatus.Active, "Active")]
    [InlineData(TestStatus.Inactive, "Inactive")]
    [InlineData(TestStatus.Pending, "Pending")]
    public void ToDisplayName_WithoutDisplayAttribute_ShouldReturnEnumName(TestStatus input, string expected)
    {
        // Act
        var result = input.ToDisplayName();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(TestStatusWithDescription.Active, "Descrição do Ativo")]
    [InlineData(TestStatusWithDescription.Inactive, "Descrição do Inativo")]
    [InlineData(TestStatusWithDescription.Pending, "Descrição do Pendente")]
    public void ToDescription_WithDescriptionAttribute_ShouldReturnDescriptionValue(TestStatusWithDescription input, string expected)
    {
        // Act
        var result = input.ToDescription();

        // Assert
        result.Should().Be(expected);
    }
}
