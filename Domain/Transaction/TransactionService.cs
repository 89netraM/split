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
    public async Task<TransactionAggregate> CreateTransactionAsync(
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

        var transaction = new TransactionAggregate(
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

    public async Task RemoveTransactionAsync(TransactionId transactionId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Removing transaction with ID {TransactionId}", transactionId);

        var transaction = await transactionRepository.GetTransactionByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
        {
            logger.LogDebug("Cannot remove transaction ({TransactionId}) that does not exist", transactionId);
            return;
        }

        transaction.Remove(timeProvider.GetUtcNow());

        await transactionRepository.SaveAsync(transaction, cancellationToken);
        logger.LogDebug("Successfully removed transaction with ID: {TransactionId}", transactionId);
    }
}
