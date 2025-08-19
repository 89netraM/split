using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Split.Domain.Primitives;
using Split.Domain.Transaction;

namespace Split.Infrastructure.Repositories;

public class TransactionRepository(SplitDbContext dbContext, IMediator mediator) : ITransactionRepository
{
    public async Task<TransactionAggregate?> GetTransactionByIdAsync(
        TransactionId transactionId,
        CancellationToken cancellationToken
    ) =>
        await dbContext.Transactions.FirstOrDefaultAsync(
            t => t.Id == transactionId && t.RemovedAt == null,
            cancellationToken
        );

    public IAsyncEnumerable<TransactionAggregate> GetTransactionsInvolvingUserAsync(
        UserId userId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Transactions.Where(t =>
                (t.SenderId == userId || t.RecipientIds.Any(rId => rId == userId)) && t.RemovedAt == null
            )
            .AsAsyncEnumerable();

    public async Task SaveAsync(TransactionAggregate transaction, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(transaction) is null or { State: EntityState.Detached })
        {
            dbContext.Add(transaction);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            transaction
                .FlushDomainEvents()
                .Select(domainEvent => mediator.Publish(domainEvent, cancellationToken).AsTask())
        );
    }
}
