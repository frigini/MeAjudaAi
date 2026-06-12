using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.Validators;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Validators;

public class CreateEmailTemplateCommandValidatorTests
{
    private readonly CreateEmailTemplateCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Key_Is_Empty()
    {
        var command = new CreateEmailTemplateCommand(string.Empty, "Subject", "<p>Hi</p>", "Hi", false, "pt", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Key));
    }

    [Fact]
    public void Should_Have_Error_When_Key_Exceeds_Max_Length()
    {
        var command = new CreateEmailTemplateCommand(new string('a', 101), "Subject", "<p>Hi</p>", "Hi", false, "pt", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Key));
    }

    [Fact]
    public void Should_Have_Error_When_Key_Contains_Invalid_Characters()
    {
        var command = new CreateEmailTemplateCommand("Invalid Key!", "Subject", "<p>Hi</p>", "Hi", false, "pt", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Key));
    }

    [Theory]
    [InlineData("welcome_template")]
    [InlineData("email_v1")]
    [InlineData("docverified")]
    public void Should_Not_Have_Error_When_Key_Is_Valid(string key)
    {
        var command = new CreateEmailTemplateCommand(key, "Subject", "<p>Hi</p>", "Hi", false, "pt", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Key));
    }

    [Fact]
    public void Should_Have_Error_When_Subject_Is_Empty()
    {
        var command = new CreateEmailTemplateCommand("valid_key", string.Empty, "<p>Hi</p>", "Hi", false, "pt", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Subject));
    }

    [Fact]
    public void Should_Have_Error_When_HtmlBody_Is_Empty()
    {
        var command = new CreateEmailTemplateCommand("valid_key", "Subject", string.Empty, "Hi", false, "pt", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.HtmlBody));
    }

    [Fact]
    public void Should_Have_Error_When_TextBody_Is_Empty()
    {
        var command = new CreateEmailTemplateCommand("valid_key", "Subject", "<p>Hi</p>", string.Empty, false, "pt", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.TextBody));
    }

    [Fact]
    public void Should_Have_Error_When_Language_Is_Empty()
    {
        var command = new CreateEmailTemplateCommand("valid_key", "Subject", "<p>Hi</p>", "Hi", false, string.Empty, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Language));
    }

    [Theory]
    [InlineData("pt")]
    [InlineData("en")]
    [InlineData("pt-br")]
    [InlineData("en-us")]
    public void Should_Not_Have_Error_When_Language_Is_Valid(string language)
    {
        var command = new CreateEmailTemplateCommand("valid_key", "Subject", "<p>Hi</p>", "Hi", false, language, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Language));
    }

    [Fact]
    public void Should_Have_Error_When_Language_Is_Invalid_Format()
    {
        var command = new CreateEmailTemplateCommand("valid_key", "Subject", "<p>Hi</p>", "Hi", false, "portuguese", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateEmailTemplateCommand.Language));
    }

    [Fact]
    public void Should_Be_Valid_When_All_Fields_Are_Valid()
    {
        var command = new CreateEmailTemplateCommand("welcome", "Welcome!", "<p>Hello!</p>", "Hello!", false, "pt-br", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
