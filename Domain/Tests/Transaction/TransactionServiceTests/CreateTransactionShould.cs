using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
using Split.Domain.Transaction;
using Split.Utilities;

namespace Split.Domain.Tests.Transaction.TransactionServiceTests;

[TestClass]
public class CreateTransactionShould
{
    [TestMethod]
    public async Task SuccessfullyCreateATransaction()
    {
        // Arrange
        var amount = new Money(250, new("SEK"));
        var description = "Lunch";
        var senderId = new UserId("7fcb3be6-e921-41ab-b1e4-02574c7d58ec");
        var recipientIds = new NonEmptyImmutableSet<UserId>(
            senderId,
            new UserId("f56f9972-6ead-4554-971a-bf5952f6320f")
        );
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 10, 51, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var service = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            Substitute.For<ITransactionRepository>()
        );

        // Act
        var transaction = await service.CreateTransactionAsync(
            amount,
            description,
            senderId,
            recipientIds,
            CancellationToken.None
        );

        // Assert
        Assert.IsNotNull(transaction);
        Assert.AreEqual(amount, transaction.Amount);
        Assert.AreEqual(description, transaction.Description);
        Assert.AreEqual(senderId, transaction.SenderId);
        Assert.AreEqual(recipientIds, transaction.RecipientIds);
        Assert.IsTrue(transaction.CreatedAt < timeProvider.GetUtcNow(), "CreatedAt should be in the past");
        Assert.IsNull(transaction.RemovedAt);
    }
}
