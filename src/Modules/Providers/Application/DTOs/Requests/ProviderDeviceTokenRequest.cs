using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.DTOs.Requests;

/// <summary>
/// Request para atualização do token de dispositivo do prestador.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ProviderDeviceTokenRequest(string DeviceToken);
