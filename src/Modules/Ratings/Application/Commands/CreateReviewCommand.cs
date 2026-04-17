using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.Application.Commands;

[ExcludeFromCodeCoverage]

public record CreateReviewCommand(
    Guid ProviderId,
    Guid CustomerId,
    int Rating,
    string? Comment
) : ICommand<Guid>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}
