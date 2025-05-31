using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
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
            new(new("52710472-f392-4d1a-9a5c-727bc365346a")),
            timeProvider.GetUtcNow()
        );
        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository
            .GetTransactionByIdAsync(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            transactionRepository
        );

        // Act
        await transactionService.RemoveTransactionAsync(transaction.Id, CancellationToken.None);

        // Assert
        await transactionRepository.Received(1).SaveAsync(transaction, Arg.Any<CancellationToken>());
        Assert.IsNotNull(transaction.RemovedAt);
    }

    [TestMethod]
    public async Task BeIdempotent_ByNotFailingWhenTheTransactionDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 11, 22, 00, new(02, 00, 00)));
        var transactionId = new TransactionId(new("ad762c8d-2bd6-4c76-b0c7-6a4366874979"));
        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository
            .GetTransactionByIdAsync(transactionId, Arg.Any<CancellationToken>())
            .Returns((TransactionAggregate?)null);
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            transactionRepository
        );

        // Act
        await transactionService.RemoveTransactionAsync(transactionId, CancellationToken.None);

        // Assert
        await transactionRepository
            .DidNotReceive()
            .SaveAsync(Arg.Any<TransactionAggregate>(), Arg.Any<CancellationToken>());
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
            new(new("2d2a7bdd-160e-42c0-bb9b-1785c4a8937e")),
            timeProvider.GetUtcNow()
        );
        var removedAt = timeProvider.GetUtcNow();
        transaction.Remove(removedAt);
        var transactionRepository = Substitute.For<ITransactionRepository>();
        transactionRepository
            .GetTransactionByIdAsync(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(transaction);
        var transactionService = new TransactionService(
            new NullLogger<TransactionService>(),
            timeProvider,
            transactionRepository
        );

        // Act
        await transactionService.RemoveTransactionAsync(transaction.Id, CancellationToken.None);

        // Assert
        await transactionRepository.Received(1).SaveAsync(transaction, Arg.Any<CancellationToken>());
        Assert.AreEqual(removedAt, transaction.RemovedAt);
    }
}
