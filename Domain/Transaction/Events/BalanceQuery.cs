using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.Transaction.Events;

public record BalanceQuery(UserId UserId) : IQuery<BalanceResponse>;

public record BalanceResponse(Balance[] Balances);

public class BalanceQueryHandler(TransactionService transactionService) : IQueryHandler<BalanceQuery, BalanceResponse>
{
    public async ValueTask<BalanceResponse> Handle(BalanceQuery query, CancellationToken cancellationToken) =>
        new(await transactionService.GetBalanceForUserAsync(query.UserId, cancellationToken));
}
