using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para atualizar o token do dispositivo para notificações push do prestador.
/// </summary>
public sealed record UpdateProviderDeviceTokenCommand(
    Guid ProviderId,
    string DeviceToken
) : Command<Result>;
