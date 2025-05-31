using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Split.Domain.Primitives;
using Split.Domain.Transaction;

namespace Split.Domain.Tests.TestCommon;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly Dictionary<TransactionId, TransactionAggregate> transactions = [];

    public Task<TransactionAggregate?> GetTransactionByIdAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken
    ) => Task.FromResult(transactions.TryGetValue(transactionId, out var transaction) ? transaction : null);

    public IAsyncEnumerable<TransactionAggregate> GetTransactionsInvolvingUserAsync(
        UserId userId,
        CancellationToken cancellationToken
    ) =>
        transactions
            .Values.Where(transaction =>
                transaction.RemovedAt is null
                && (transaction.SenderId == userId || transaction.RecipientIds.Contains(userId))
            )
            .ToAsyncEnumerable();

    public Task SaveAsync(TransactionAggregate transaction, CancellationToken cancellationToken)
    {
        transactions[transaction.Id] = transaction;
        return Task.CompletedTask;
    }
}
