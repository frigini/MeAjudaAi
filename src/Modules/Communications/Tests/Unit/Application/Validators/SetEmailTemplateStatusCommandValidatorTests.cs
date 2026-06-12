using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.Validators;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Validators;

public class SetEmailTemplateStatusCommandValidatorTests
{
    private readonly SetEmailTemplateStatusCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var command = new SetEmailTemplateStatusCommand(Guid.Empty, true, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetEmailTemplateStatusCommand.Id));
    }

    [Fact]
    public void Should_Be_Valid_When_Id_Is_Provided()
    {
        var command = new SetEmailTemplateStatusCommand(Guid.NewGuid(), true, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Be_Valid_When_IsActive_Is_False()
    {
        var command = new SetEmailTemplateStatusCommand(Guid.NewGuid(), false, Guid.NewGuid());
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
