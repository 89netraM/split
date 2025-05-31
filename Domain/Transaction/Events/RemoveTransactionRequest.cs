using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.Transaction.Events;

public record RemoveTransactionRequest(TransactionId TransactionId) : IRequest<RemoveTransactionResponse>;

public record RemoveTransactionResponse();

public class RemoveTransactionRequestHandler(TransactionService transactionService)
    : IRequestHandler<RemoveTransactionRequest, RemoveTransactionResponse>
{
    public async ValueTask<RemoveTransactionResponse> Handle(
        RemoveTransactionRequest request,
        CancellationToken cancellationToken
    )
    {
        await transactionService.RemoveTransactionAsync(request.TransactionId, cancellationToken);

        return new RemoveTransactionResponse();
    }
}
