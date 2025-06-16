using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Domain.Transaction;

public class TransactionService(
    ILogger<TransactionService> logger,
    TimeProvider timeProvider,
    ITransactionRepository transactionRepository,
    IUserRepository userRepository
)
{
    public async Task<TransactionAggregate> CreateTransactionAsync(
        Money amount,
        string? description,
        UserId senderId,
        IReadOnlyList<UserId> recipientIds,
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

        if (await userRepository.GetUserByIdAsync(senderId, cancellationToken) is null)
        {
            throw new SenderNotFoundException(senderId);
        }
        var missingRecipients = await recipientIds
            .ToAsyncEnumerable()
            .WhereAwait(async recipientId =>
                await userRepository.GetUserByIdAsync(recipientId, cancellationToken) is null
            )
            .ToArrayAsync(cancellationToken);
        if (missingRecipients is not [])
        {
            throw new RecipientsNotFoundException(missingRecipients);
        }

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

    public IAsyncEnumerable<TransactionAggregate> GetTransactionsInvolvingUserAsync(
        UserId userId,
        CancellationToken cancellationToken
    )
    {
        logger.LogDebug("Retrieving transactions involving user ID {UserId}", userId);

        return transactionRepository.GetTransactionsInvolvingUserAsync(userId, cancellationToken);
    }

    public IAsyncEnumerable<Balance> GetBalanceForUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving balance for user ID {UserId}", userId);

        return transactionRepository
            .GetTransactionsInvolvingUserAsync(userId, cancellationToken)
            .SelectMany(transaction =>
                transaction
                    .RecipientIds.Select(to =>
                        transaction.SenderId == to
                            ? new Balance(to, transaction.SenderId, new(0.0m, transaction.Amount.Currency))
                            : new Balance(transaction.SenderId, to, transaction.Amount / transaction.RecipientIds.Count)
                    )
                    .ToAsyncEnumerable()
            )
            .GroupByAwait(info => ValueTask.FromResult(info.To != userId ? info.To : info.From), AggregateBalance)
            .SelectMany(g => g.ToAsyncEnumerable());

        static async ValueTask<IEnumerable<Balance>> AggregateBalance(UserId _, IAsyncEnumerable<Balance> group)
        {
            var balanceByCurrency = new Dictionary<Currency, Balance>();
            await foreach (var balanceToAdd in group)
            {
                if (balanceByCurrency.TryGetValue(balanceToAdd.Amount.Currency, out var balance))
                {
                    var amount =
                        balanceToAdd.To == balance.To
                            ? balance.Amount.Amount + balanceToAdd.Amount.Amount
                            : balance.Amount.Amount - balanceToAdd.Amount.Amount;
                    if (amount >= 0)
                    {
                        balance = new Balance(balance.From, balance.To, new Money(amount, balance.Amount.Currency));
                    }
                    else
                    {
                        balance = new Balance(balance.To, balance.From, new Money(-amount, balance.Amount.Currency));
                    }
                }
                else
                {
                    balance = new Balance(balanceToAdd.From, balanceToAdd.To, balanceToAdd.Amount);
                }
                balanceByCurrency[balance.Amount.Currency] = balance;
            }
            return balanceByCurrency.Values;
        }
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

public class SenderNotFoundException(UserId senderId) : Exception($"A user with the id {senderId} does not exist");

public class RecipientsNotFoundException(UserId[] recipientIds)
    : Exception($"Users with ids {string.Join(", ", recipientIds.Select(id => id.Value))} does not exist");
