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
public class GetBalanceForUserAsync
{
    [TestMethod]
    public async Task ReturnBalancesForUser()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 15, 31, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserId("user-A");
        var userB = new UserId("user-B");
        var transaction1 = new TransactionAggregate(
            "Test Transaction 1",
            new(100, new("SEK")),
            userA,
            new(userB),
            timeProvider.GetUtcNow()
        );
        var transaction2 = new TransactionAggregate(
            "Test Transaction 2",
            new(50, new("SEK")),
            userB,
            new(userA),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository(transaction1, transaction2);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService.GetBalanceForUserAsync(userA, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);

        var balance = result[0];
        Assert.AreEqual(userA, balance.From);
        Assert.AreEqual(userB, balance.To);
        Assert.AreEqual(50, balance.Amount.Amount);
        Assert.AreEqual("SEK", balance.Amount.Currency.Value);
    }

    [TestMethod]
    public async Task ReturnBalancesForUser_WhenBalanceDipsBelowZero()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 15, 31, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserId("user-A");
        var userB = new UserId("user-B");
        var transaction1 = new TransactionAggregate(
            "Test Transaction 1",
            new(100, new("SEK")),
            userA,
            new(userB),
            timeProvider.GetUtcNow()
        );
        var transaction2 = new TransactionAggregate(
            "Test Transaction 2",
            new(150, new("SEK")),
            userB,
            new(userA),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository(transaction1, transaction2);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService.GetBalanceForUserAsync(userA, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);

        var balance = result[0];
        Assert.AreEqual(userB, balance.From);
        Assert.AreEqual(userA, balance.To);
        Assert.AreEqual(50, balance.Amount.Amount);
        Assert.AreEqual("SEK", balance.Amount.Currency.Value);
    }

    [TestMethod]
    public async Task ReturnBalancesThatAreZero()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 15, 31, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserId("user-A");
        var userB = new UserId("user-B");
        var transaction1 = new TransactionAggregate(
            "Test Transaction 1",
            new(100, new("SEK")),
            userA,
            new(userB),
            timeProvider.GetUtcNow()
        );
        var transaction2 = new TransactionAggregate(
            "Test Transaction 2",
            new(100, new("SEK")),
            userB,
            new(userA),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository(transaction1, transaction2);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService.GetBalanceForUserAsync(userA, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);

        var balance = result[0];
        Assert.AreEqual(0, balance.Amount.Amount);
    }

    [TestMethod]
    public async Task ReturnMultipleBalancesForSameUserPair_WhenPairHasMadeTransactionsInMultipleCurrencies()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 15, 31, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserId("user-A");
        var userB = new UserId("user-B");
        var transaction1 = new TransactionAggregate(
            "Test Transaction 1",
            new(100, new("SEK")),
            userA,
            new(userB),
            timeProvider.GetUtcNow()
        );
        var transaction2 = new TransactionAggregate(
            "Test Transaction 2",
            new(50, new("EUR")),
            userB,
            new(userA),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository(transaction1, transaction2);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService.GetBalanceForUserAsync(userA, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(2, result.Length);

        Assert.IsTrue(
            result.Any(balance =>
                balance.From == userA
                && balance.To == userB
                && balance.Amount.Amount == 100
                && balance.Amount.Currency.Value == "SEK"
            )
        );
        Assert.IsTrue(
            result.Any(balance =>
                balance.From == userB
                && balance.To == userA
                && balance.Amount.Amount == 50
                && balance.Amount.Currency.Value == "EUR"
            )
        );
    }

    [TestMethod]
    public async Task ReturnMultipleBalances_WhenUserHadMadeTransactionsWithMultipleUsers()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 15, 31, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserId("user-A");
        var userB = new UserId("user-B");
        var userC = new UserId("user-C");
        var transaction1 = new TransactionAggregate(
            "Test Transaction 1",
            new(100, new("SEK")),
            userA,
            new(userB),
            timeProvider.GetUtcNow()
        );
        var transaction2 = new TransactionAggregate(
            "Test Transaction 2",
            new(50, new("SEK")),
            userA,
            new(userC),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository(transaction1, transaction2);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService.GetBalanceForUserAsync(userA, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(2, result.Length);

        Assert.IsTrue(
            result.Any(balance =>
                balance.From == userA
                && balance.To == userB
                && balance.Amount.Amount == 100
                && balance.Amount.Currency.Value == "SEK"
            )
        );
        Assert.IsTrue(
            result.Any(balance =>
                balance.From == userA
                && balance.To == userC
                && balance.Amount.Amount == 50
                && balance.Amount.Currency.Value == "SEK"
            )
        );
    }

    [TestMethod]
    public async Task NotReturnUnrelatedBalances()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 15, 31, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserId("user-A");
        var userB = new UserId("user-B");
        var userC = new UserId("user-C");
        var transaction1 = new TransactionAggregate(
            "Test Transaction 1",
            new(100, new("SEK")),
            userA,
            new(userB),
            timeProvider.GetUtcNow()
        );
        var transaction2 = new TransactionAggregate(
            "Test Transaction 2",
            new(50, new("SEK")),
            userB,
            new(userC),
            timeProvider.GetUtcNow()
        );

        var repository = new InMemoryTransactionRepository(transaction1, transaction2);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        // Act
        var result = await transactionService.GetBalanceForUserAsync(userA, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.IsFalse(result.Any(balance => balance.From != userA && balance.To != userA));
    }
}
