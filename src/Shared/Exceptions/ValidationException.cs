using FluentValidation.Results;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Exceptions;

[ExcludeFromCodeCoverage]

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
