using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Users.DTOs;

/// <summary>
/// DTO para atualização do token do dispositivo para notificações push.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DeviceTokenRequestDto(string? DeviceToken);
