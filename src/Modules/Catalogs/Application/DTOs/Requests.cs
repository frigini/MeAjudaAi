using MeAjudaAi.Shared.Contracts;

namespace MeAjudaAi.Modules.Catalogs.Application.DTOs.Requests;

// ============================================================================
// SERVICE CATEGORY REQUESTS
// ============================================================================

public sealed record UpdateServiceCategoryRequest : Request
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
}

// ============================================================================
// SERVICE REQUESTS
// ============================================================================

public sealed record CreateServiceRequest : Request
{
    public Guid CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; } = 0;
}

public sealed record UpdateServiceRequest : Request
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
}

public sealed record ChangeServiceCategoryRequest : Request
{
    public Guid NewCategoryId { get; init; }
}
