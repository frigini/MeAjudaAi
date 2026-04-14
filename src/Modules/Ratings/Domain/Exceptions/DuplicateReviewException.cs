using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.Ratings.Domain.Exceptions;

public class DuplicateReviewException : BusinessRuleException
{
    public DuplicateReviewException(Guid providerId, Guid customerId)
        : base("DuplicateReview", $"O cliente '{customerId}' já avaliou o prestador '{providerId}'.")
    {
    }
}
