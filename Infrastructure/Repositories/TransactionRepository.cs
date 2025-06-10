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
        await dbContext.Transactions //.Include(t => t.RecipientIds)
        .FirstOrDefaultAsync(u => u.Id == transactionId, cancellationToken);

    public IAsyncEnumerable<TransactionAggregate> GetTransactionsInvolvingUserAsync(
        UserId userId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Transactions //.Include(t => t.RecipientIds)
            .Where(t => t.SenderId == userId || t.RecipientIds.Any(rId => rId == userId))
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
