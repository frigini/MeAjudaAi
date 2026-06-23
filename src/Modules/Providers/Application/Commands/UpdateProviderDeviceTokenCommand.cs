using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Providers.Application.Commands;

/// <summary>
/// Comando para atualizar o token do dispositivo para notificações push do prestador.
/// </summary>
public sealed record UpdateProviderDeviceTokenCommand(
    Guid ProviderId,
    string DeviceToken
) : Command<Result>;
