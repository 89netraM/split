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

namespace Split.Domain.Tests.Transaction.Events.UserTransactionsQuery;

[TestClass]
public class HandleShould
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

        var repository = new InMemoryTransactionRepository(transaction1, transaction2);

        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            repository,
            new InMemoryUserRepository()
        );

        var handler = new UserTransactionsRequestHandler(transactionService);

        // Act
        var result = await handler.Handle(new(userId), CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(result.Any(r => r.Transaction.Id == transaction1.Id));
        Assert.IsTrue(result.Any(r => r.Transaction.Id == transaction2.Id));
    }
}
