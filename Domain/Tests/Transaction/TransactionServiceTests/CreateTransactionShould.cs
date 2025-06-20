using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Domain.Tests.TestCommon;
using Split.Domain.Transaction;
using Split.Domain.User;

namespace Split.Domain.Tests.Transaction.TransactionServiceTests;

[TestClass]
public class CreateTransactionShould
{
    [TestMethod]
    public async Task SuccessfullyCreateATransaction()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 02, 20, 12, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };

        var amount = new Money(250, new("SEK"));
        var description = "Lunch";
        var sender = new UserAggregate(new("user-sender"), "Sender", new("+0123456789"), timeProvider.GetUtcNow());
        var recipient = new UserAggregate(
            new("user-recipient"),
            "Recipient",
            new("+9876543210"),
            timeProvider.GetUtcNow()
        );

        var userRepository = new InMemoryUserRepository(sender, recipient);
        UserId[] recipientIds = [sender.Id, recipient.Id];
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            new InMemoryTransactionRepository(),
            userRepository
        );

        // Act
        var transaction = await transactionService.CreateTransactionAsync(
            amount,
            description,
            sender.Id,
            recipientIds,
            CancellationToken.None
        );

        // Assert
        Assert.IsNotNull(transaction);
        Assert.AreEqual(amount, transaction.Amount);
        Assert.AreEqual(description, transaction.Description);
        Assert.AreEqual(sender.Id, transaction.SenderId);
        Assert.IsTrue(recipientIds.ToHashSet().SetEquals(transaction.RecipientIds));
        Assert.IsTrue(transaction.CreatedAt < timeProvider.GetUtcNow(), "CreatedAt should be in the past");
        Assert.IsNull(transaction.RemovedAt);
    }

    [TestMethod]
    public async Task ThrowAnException_WhenTheSenderDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 02, 20, 12, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };

        var amount = new Money(250, new("SEK"));
        var description = "Lunch";
        var senderId = new UserId("user-sender");
        var recipient = new UserAggregate(
            new("user-recipient"),
            "Recipient",
            new("+9876543210"),
            timeProvider.GetUtcNow()
        );

        var userRepository = new InMemoryUserRepository(recipient);
        UserId[] recipientIds = [senderId, recipient.Id];
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            new InMemoryTransactionRepository(),
            userRepository
        );

        // Act
        var action = async () =>
            await transactionService.CreateTransactionAsync(
                amount,
                description,
                senderId,
                recipientIds,
                CancellationToken.None
            );

        // Assert
        await Assert.ThrowsExceptionAsync<SenderNotFoundException>(action);
    }

    [TestMethod]
    public async Task ThrowAnException_WhenARecipientDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 02, 20, 12, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };

        var amount = new Money(250, new("SEK"));
        var description = "Lunch";
        var sender = new UserAggregate(new("user-sender"), "Sender", new("+0123456789"), timeProvider.GetUtcNow());
        var recipientId = new UserId("user-recipient");

        var userRepository = new InMemoryUserRepository(sender);
        UserId[] recipientIds = [sender.Id, recipientId];
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            new InMemoryTransactionRepository(),
            userRepository
        );

        // Act
        var action = async () =>
            await transactionService.CreateTransactionAsync(
                amount,
                description,
                sender.Id,
                recipientIds,
                CancellationToken.None
            );

        // Assert
        await Assert.ThrowsExceptionAsync<RecipientsNotFoundException>(action);
    }
}
