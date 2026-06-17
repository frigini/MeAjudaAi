using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Commands;

namespace MeAjudaAi.Modules.Ratings.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands do módulo Ratings.
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia CreateReviewRequest para CreateReviewCommand.
    /// </summary>
    public static CreateReviewCommand ToCommand(this CreateReviewRequest request, Guid customerId)
    {
        return new CreateReviewCommand(
            request.ProviderId,
            customerId,
            request.Rating,
            request.Comment);
    }
}
