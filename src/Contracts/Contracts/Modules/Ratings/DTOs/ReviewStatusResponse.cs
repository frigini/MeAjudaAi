using MeAjudaAi.Contracts.Contracts.Modules.Ratings.Enums;

namespace MeAjudaAi.Contracts.Contracts.Modules.Ratings.DTOs;

public record ReviewStatusResponse(Guid Id, EReviewStatus Status);
