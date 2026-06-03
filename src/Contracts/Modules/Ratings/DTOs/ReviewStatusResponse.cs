using MeAjudaAi.Contracts.Modules.Ratings.Enums;

namespace MeAjudaAi.Contracts.Modules.Ratings.DTOs;

public record ReviewStatusResponse(Guid Id, EReviewStatus Status);
