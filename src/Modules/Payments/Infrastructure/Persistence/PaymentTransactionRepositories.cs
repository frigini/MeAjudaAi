using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

public partial class PaymentsDbContext : IRepository<PaymentTransaction, Guid>
{
    async Task<PaymentTransaction?> IRepository<PaymentTransaction, Guid>.TryFindAsync(Guid key, CancellationToken cancellationToken) =>
        await PaymentTransactions.FirstOrDefaultAsync(t => t.Id == key, cancellationToken);

    void IRepository<PaymentTransaction, Guid>.Add(PaymentTransaction aggregate) => PaymentTransactions.Add(aggregate);
    void IRepository<PaymentTransaction, Guid>.Delete(PaymentTransaction aggregate) => PaymentTransactions.Remove(aggregate);
}