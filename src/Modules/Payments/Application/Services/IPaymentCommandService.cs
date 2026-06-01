using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Payments.Application.Services;

public interface IPaymentCommandService
{
    Task<Result> SaveInboxMessageAsync(string type, string content, string externalEventId, CancellationToken ct = default);
}
