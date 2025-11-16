using FluentValidation;
using MeAjudaAi.Modules.Search.Application.Queries;

namespace MeAjudaAi.Modules.Search.Application.Validators;

/// <summary>
/// Validador para SearchProvidersQuery.
/// </summary>
public sealed class SearchProvidersQueryValidator : AbstractValidator<SearchProvidersQuery>
{
    public SearchProvidersQueryValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90 degrees.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180 degrees.");

        RuleFor(x => x.RadiusInKm)
            .GreaterThan(0)
            .WithMessage("Radius must be greater than 0.")
            .LessThanOrEqualTo(500)
            .WithMessage("Radius cannot exceed 500 km.");

        RuleFor(x => x.MinRating)
            .InclusiveBetween(0, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("Minimum rating must be between 0 and 5.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100 items.");
    }
}
