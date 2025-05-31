using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Split.Domain.Primitives;

namespace Split.Domain.Transaction;

public interface ITransactionRepository
{
    Task<TransactionAggregate?> GetTransactionByIdAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken
    );
    IAsyncEnumerable<TransactionAggregate> GetTransactionsInvolvingUserAsync(
        UserId userId,
        CancellationToken cancellationToken
    );
    Task SaveAsync(TransactionAggregate transaction, CancellationToken cancellationToken);
}
