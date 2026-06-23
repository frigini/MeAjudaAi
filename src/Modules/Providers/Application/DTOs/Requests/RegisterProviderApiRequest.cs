using MeAjudaAi.Modules.Providers.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public record RegisterProviderApiRequest(
    [Required, StringLength(100)] string Name,
    [Required, EnumDataType(typeof(EProviderType))] EProviderType Type,
    [Required, StringLength(20)] string DocumentNumber,
    [Phone, StringLength(20)] string? PhoneNumber
);
