using MeAjudaAi.Contracts.Modules.Ratings.Enums;

namespace MeAjudaAi.Contracts.Modules.Ratings.DTOs;

public sealed record ReviewStatusResponse(Guid Id, EReviewStatus Status);
