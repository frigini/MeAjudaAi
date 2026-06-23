namespace MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;

public interface IPaymentsHealthQueries
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}