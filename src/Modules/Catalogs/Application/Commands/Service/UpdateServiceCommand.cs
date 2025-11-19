using System.ComponentModel.DataAnnotations;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands.Service;

/// <summary>
/// Command to update an existing service's details.
/// Validation limits must match ValidationConstants.CatalogLimits.
/// Note: Guid.Empty validation is handled by the command handler to provide domain-specific error messages.
/// </summary>
public sealed record UpdateServiceCommand(
    Guid Id,
    [Required]
    [MaxLength(150)]
    string Name,
    [MaxLength(1000)]
    string? Description,
    [Range(0, int.MaxValue)]
    int DisplayOrder
) : Command<Result>;
