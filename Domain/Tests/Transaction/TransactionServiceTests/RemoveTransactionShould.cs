using System;
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
public class RemoveTransactionShould
{
    [TestMethod]
    public async Task SuccessfullyRemoveATransaction()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 11, 22, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var transaction = new TransactionAggregate(
            "Test Transaction",
            new(100, new("SEK")),
            new("1c9dfcc8-c06a-4e06-b44e-69bb05741bdc"),
            [new("52710472-f392-4d1a-9a5c-727bc365346a")],
            timeProvider.GetUtcNow()
        );
        var transactionRepository = new InMemoryTransactionRepository(transaction);
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            transactionRepository,
            new InMemoryUserRepository()
        );

        // Act
        await transactionService.RemoveTransactionAsync(transaction.Id, CancellationToken.None);

        // Assert
        Assert.IsNotNull(transaction.RemovedAt);
    }

    [TestMethod]
    public async Task BeIdempotent_ByNotFailingWhenTheTransactionDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 11, 22, 00, new(02, 00, 00)));
        var transactionId = new TransactionId(new("ad762c8d-2bd6-4c76-b0c7-6a4366874979"));
        var transactionRepository = new InMemoryTransactionRepository();
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            transactionRepository,
            new InMemoryUserRepository()
        );

        // Act & Assert
        await transactionService.RemoveTransactionAsync(transactionId, CancellationToken.None);
    }

    [TestMethod]
    public async Task BeIdempotent_ByNotFailingWhenTheTransactionIsAlreadyRemoved()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 11, 22, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var transaction = new TransactionAggregate(
            "Test Transaction",
            new(100, new("SEK")),
            new("d6506d23-18ea-4c19-9817-898d24e3cedb"),
            [new("2d2a7bdd-160e-42c0-bb9b-1785c4a8937e")],
            timeProvider.GetUtcNow()
        );
        var removedAt = timeProvider.GetUtcNow();
        transaction.Remove(removedAt);
        var transactionRepository = new InMemoryTransactionRepository(transaction);
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            transactionRepository,
            new InMemoryUserRepository()
        );

        // Act
        await transactionService.RemoveTransactionAsync(transaction.Id, CancellationToken.None);

        // Assert
        Assert.AreEqual(removedAt, transaction.RemovedAt);
    }
}
