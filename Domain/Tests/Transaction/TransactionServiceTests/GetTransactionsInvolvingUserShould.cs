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

namespace Split.Domain.Tests.Transaction.TransactionServiceTests;

[TestClass]
public class GetTransactionsInvolvingUserShould
{
    [TestMethod]
    public async Task ReturnAllRelatedTransactions()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 13, 34, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userId = new UserId("user-A");
        var transaction1 = new TransactionAggregate(
            "Test Transaction 1",
            new(100, new("SEK")),
            userId,
            new(new("user-B")),
            timeProvider.GetUtcNow()
        );
        var transaction2 = new TransactionAggregate(
            "Test Transaction 2",
            new(200, new("SEK")),
            new UserId("user-B"),
            new(userId, new UserId("user-C")),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository();
        await repository.SaveAsync(transaction1, CancellationToken.None);
        await repository.SaveAsync(transaction2, CancellationToken.None);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService
            .GetTransactionsInvolvingUserAsync(userId, CancellationToken.None)
            .ToArrayAsync();

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(result.Any(transaction => transaction.Id == transaction1.Id));
        Assert.IsTrue(result.Any(transaction => transaction.Id == transaction2.Id));
    }

    [TestMethod]
    public async Task NotReturnAnyRelatedButRemovedTransactions()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 13, 34, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userId = new UserId("user-A");
        var transaction = new TransactionAggregate(
            "Test Transaction",
            new(100, new("SEK")),
            userId,
            new(new("user-B")),
            timeProvider.GetUtcNow()
        );
        var removedTransaction = new TransactionAggregate(
            "Removed Transaction",
            new(100, new("SEK")),
            userId,
            new(new("user-B")),
            timeProvider.GetUtcNow()
        );
        removedTransaction.Remove(timeProvider.GetUtcNow());

        var repository = new InMemoryTransactionRepository();
        await repository.SaveAsync(transaction, CancellationToken.None);
        await repository.SaveAsync(removedTransaction, CancellationToken.None);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService
            .GetTransactionsInvolvingUserAsync(userId, CancellationToken.None)
            .ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.IsFalse(result.Any(transaction => transaction.Id == removedTransaction.Id));
    }

    [TestMethod]
    public async Task NotReturnAnyUnrelatedTransactions()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 13, 34, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userId = new UserId("user-A");
        var transaction = new TransactionAggregate(
            "Test Transaction",
            new(100, new("SEK")),
            userId,
            new(new("user-B")),
            timeProvider.GetUtcNow()
        );
        var unrelatedTransaction = new TransactionAggregate(
            "Unrelated Transaction",
            new(100, new("SEK")),
            new("user-B"),
            new(new("user-C")),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository();
        await repository.SaveAsync(transaction, CancellationToken.None);
        await repository.SaveAsync(unrelatedTransaction, CancellationToken.None);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService
            .GetTransactionsInvolvingUserAsync(userId, CancellationToken.None)
            .ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.IsFalse(result.Any(transaction => transaction.Id == unrelatedTransaction.Id));
    }
}
