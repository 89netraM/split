using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.Transaction.Events;

public record UserTransactionsQuery(UserId UserId) : IStreamQuery<UserTransactionsResponse>;

public record UserTransactionsResponse(TransactionAggregate Transaction);

public class UserTransactionsRequestHandler(TransactionService transactionService)
    : IStreamQueryHandler<UserTransactionsQuery, UserTransactionsResponse>
{
    public IAsyncEnumerable<UserTransactionsResponse> Handle(
        UserTransactionsQuery query,
        CancellationToken cancellationToken
    ) =>
        transactionService
            .GetTransactionsInvolvingUserAsync(query.UserId, cancellationToken)
            .Select(transaction => new UserTransactionsResponse(transaction));
}
