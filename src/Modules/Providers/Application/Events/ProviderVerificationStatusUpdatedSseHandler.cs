using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Streaming;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Events;

public class ProviderVerificationStatusUpdatedSseHandler(
    ISseHub<ProviderVerificationSseDto> sseHub,
    ILogger<ProviderVerificationStatusUpdatedSseHandler> logger) :
    IEventHandler<ProviderVerificationStatusUpdatedDomainEvent>
{
    public async Task HandleAsync(ProviderVerificationStatusUpdatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var topic = SseTopic.ForProviderVerification(@event.AggregateId);
        var data = new ProviderVerificationSseDto(
            @event.AggregateId, 
            @event.NewStatus.ToString(), 
            DateTime.UtcNow);

        logger.LogInformation("Streaming update: Provider {ProviderId} verification status is now {Status}", @event.AggregateId, @event.NewStatus);
        await sseHub.PublishAsync(topic, data, cancellationToken);
    }
}
