using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Ratings.Application.Commands;

public record CreateReviewCommand(
    Guid ProviderId,
    Guid CustomerId,
    int Rating,
    string? Comment
) : ICommand<Guid>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}
