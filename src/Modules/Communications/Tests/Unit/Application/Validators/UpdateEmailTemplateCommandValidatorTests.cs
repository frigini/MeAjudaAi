using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.Validators;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Validators;

public class UpdateEmailTemplateCommandValidatorTests
{
    private readonly UpdateEmailTemplateCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new UpdateEmailTemplateCommand(Guid.Empty, "Subject", "<p>Hi</p>", "Hi", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmailTemplateCommand.Id));
    }

    [Fact]
    public void Should_Have_Error_When_Subject_Is_Empty()
    {
        var command = new UpdateEmailTemplateCommand(Guid.NewGuid(), string.Empty, "<p>Hi</p>", "Hi", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmailTemplateCommand.Subject));
    }

    [Fact]
    public void Should_Have_Error_When_HtmlBody_Is_Empty()
    {
        var command = new UpdateEmailTemplateCommand(Guid.NewGuid(), "Subject", string.Empty, "Hi", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmailTemplateCommand.HtmlBody));
    }

    [Fact]
    public void Should_Have_Error_When_TextBody_Is_Empty()
    {
        var command = new UpdateEmailTemplateCommand(Guid.NewGuid(), "Subject", "<p>Hi</p>", string.Empty, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateEmailTemplateCommand.TextBody));
    }

    [Fact]
    public void Should_Be_Valid_When_All_Fields_Are_Valid()
    {
        var command = new UpdateEmailTemplateCommand(Guid.NewGuid(), "Subject", "<p>Hi</p>", "Hi", Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
