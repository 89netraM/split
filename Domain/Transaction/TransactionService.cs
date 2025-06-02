using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;
using Split.Domain.User;
using Split.Utilities;

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

        var sender =
            await userRepository.GetUserByIdAsync(senderId, cancellationToken)
            ?? throw new SenderNotFoundException(senderId);
        if (
            recipientIds.Except([senderId, .. sender.Friendships.Select(friend => friend.FriendId)]) is
            { Count: > 0 } notFriends
        )
        {
            throw new SendingToNonFriendsException(sender.Id, notFriends);
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
                    .RecipientIds.Select(to => new Balance(
                        transaction.SenderId,
                        to,
                        transaction.Amount / transaction.RecipientIds.Count
                    ))
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

public class SenderNotFoundException(UserId senderId) : Exception($"A user with this id {senderId} does not exist");

public class SendingToNonFriendsException(UserId sender, IEnumerable<UserId> missingRecipients)
    : Exception(
        $"Sender {sender} is not friends with the following recipients: {string.Join(", ", missingRecipients)}"
    );
