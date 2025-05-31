using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;
using Split.Utilities;

namespace Split.Domain.Transaction.Events;

public record CreateTransactionRequest(
    Money Amount,
    string? Description,
    UserId SenderId,
    NonEmptyImmutableSet<UserId> RecipientIds
) : IRequest<CreateTransactionResponse>;

public record CreateTransactionResponse(TransactionAggregate Transaction);

public class CreateTransactionRequestHandler(TransactionService transactionService)
    : IRequestHandler<CreateTransactionRequest, CreateTransactionResponse>
{
    public async ValueTask<CreateTransactionResponse> Handle(
        CreateTransactionRequest request,
        CancellationToken cancellationToken
    )
    {
        var transaction = await transactionService.CreateTransactionAsync(
            request.Amount,
            request.Description,
            request.SenderId,
            request.RecipientIds,
            cancellationToken
        );

        return new CreateTransactionResponse(transaction);
    }
}
