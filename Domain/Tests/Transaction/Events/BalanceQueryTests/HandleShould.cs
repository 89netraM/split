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
using Split.Domain.Transaction.Events;

namespace Split.Domain.Tests.Transaction.Events.BalanceQueryTests;

[TestClass]
public class HandleShould
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

        var repository = new InMemoryTransactionRepository();
        await repository.SaveAsync(transaction1, CancellationToken.None);
        await repository.SaveAsync(transaction2, CancellationToken.None);

        var balanceQueryHandler = new BalanceQueryHandler(
            new TransactionService(new NullLogger<TransactionService>(), timeProvider, repository)
        );

        // Act
        var result = await balanceQueryHandler.Handle(new(userA), CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);

        var response = result[0];
        Assert.AreEqual(userA, response.Balance.From);
        Assert.AreEqual(userB, response.Balance.To);
        Assert.AreEqual(50, response.Balance.Amount.Amount);
        Assert.AreEqual("SEK", response.Balance.Amount.Currency.Value);
    }
}
