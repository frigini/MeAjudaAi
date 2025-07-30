using FluentValidation.Results;

namespace MeAjudai.Shared.Exceptions;

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = [];
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures;
    }
}