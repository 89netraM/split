using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.Transaction.Events;

public record BalanceQuery(UserId UserId) : IStreamQuery<BalanceResponse>;

public record BalanceResponse(Balance Balance);

public class BalanceQueryHandler(TransactionService transactionService)
    : IStreamQueryHandler<BalanceQuery, BalanceResponse>
{
    public IAsyncEnumerable<BalanceResponse> Handle(BalanceQuery query, CancellationToken cancellationToken) =>
        transactionService
            .GetBalanceForUserAsync(query.UserId, cancellationToken)
            .Select(balance => new BalanceResponse(balance));
}
