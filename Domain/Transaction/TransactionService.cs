using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;
using Split.Utilities;

namespace Split.Domain.Transaction;

public class TransactionService(
    ILogger<TransactionService> logger,
    TimeProvider timeProvider,
    ITransactionRepository transactionRepository
)
{
    public async Task<TransactionEntity> CreateTransactionAsync(
        Money amount,
        string? description,
        UserId senderId,
        NonEmptyImmutableSet<UserId> recipientIds,
        CancellationToken cancellationToken
    )
    {
        logger.LogDebug(
            "Creating transaction with amount {Amount}, description {Description}, sender ID {SenderId}, and recipient IDs {@RecipientIds}",
            amount,
            description,
            senderId,
            recipientIds
        );

        var transaction = new TransactionEntity(
            new(Guid.NewGuid()),
            description,
            amount,
            senderId,
            recipientIds,
            timeProvider.GetUtcNow()
        );

        await transactionRepository.SaveAsync(transaction, cancellationToken);
        logger.LogDebug("Transaction created: {TransactionId}", transaction.Id);

        return transaction;
    }
}
