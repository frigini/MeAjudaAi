using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.Application.DTOs.Requests;

[ExcludeFromCodeCoverage]
public sealed record DeviceTokenRequest(string? DeviceToken);
