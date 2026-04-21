using MeAjudaAi.Shared.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Ratings.Domain.Exceptions;

[ExcludeFromCodeCoverage]

public class DuplicateReviewException : BusinessRuleException
{
    public DuplicateReviewException(Guid providerId, Guid customerId)
        : base("DuplicateReview", $"O cliente '{customerId}' já avaliou o prestador '{providerId}'.")
    {
    }
}
