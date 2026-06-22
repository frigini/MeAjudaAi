using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Payments.Application.Services;

public interface IPaymentCommandService
{
    Task<Result> SaveInboxMessageAsync(string type, string content, string externalEventId, CancellationToken ct = default);

    Task<Result> HandleStripeWebhookAsync(
        HttpRequest request,
        CancellationToken cancellationToken = default);
}
