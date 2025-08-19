using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
using Split.Domain.Transaction;
using Split.Domain.Transaction.Events;
using Split.Domain.User;

namespace Split.Infrastructure.Tests.Repositories;

[TestClass]
public class TransactionRepositoryTests : PostgresTestBase
{
#nullable disable
    private UserAggregate userA;
    private UserAggregate userB;

#nullable restore

    [TestInitialize]
    public async Task CreateUsers()
    {
        var userRepository = Services.GetRequiredService<IUserRepository>();
        userA = new UserAggregate(
            new("user-a"),
            "User A",
            new("+9123456789"),
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );
        userB = new UserAggregate(
            new("user-b"),
            "User B",
            new("+1987654321"),
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );
        await userRepository.SaveAsync(userA, CancellationToken.None);
        await userRepository.SaveAsync(userB, CancellationToken.None);
    }

    [TestMethod]
    public async Task NothingShouldBeRetrievableWhenTheDatabaseIsEmpty()
    {
        // Arrange
        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        var transactionId = new TransactionId(new("01977ea8-61b4-7905-81d2-e6df605cc06b"));

        // Act
        var result = await transactionRepository.GetTransactionByIdAsync(transactionId, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ASavedTransactionShouldBeRetrievableById()
    {
        // Arrange
        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        var transaction = new TransactionAggregate(
            "Transaction A",
            new(100m, new("SEK")),
            userA.Id,
            [userB.Id],
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );

        // Act
        await transactionRepository.SaveAsync(transaction, CancellationToken.None);
        var result = await transactionRepository.GetTransactionByIdAsync(transaction.Id, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(transaction.Id, result.Id);
        Assert.AreEqual(transaction.Description, result.Description);
        Assert.AreEqual(transaction.SenderId, result.SenderId);
        Assert.AreEqual(transaction.Amount, result.Amount);
        Assert.AreEqual(transaction.RecipientIds.Single(), result.RecipientIds.Single());
        Assert.AreEqual(transaction.CreatedAt, result.CreatedAt);
        Assert.AreEqual(transaction.RemovedAt, result.RemovedAt);
    }

    [TestMethod]
    public async Task ARemovedTransactionShouldNotBeRetrievableById()
    {
        // Arrange
        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        var transaction = new TransactionAggregate(
            "Transaction A",
            new(100m, new("SEK")),
            userA.Id,
            [userB.Id],
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );
        transaction.Remove(new(2025, 08, 19, 15, 05, 00, new(00, 00, 00)));

        // Act
        await transactionRepository.SaveAsync(transaction, CancellationToken.None);
        var result = await transactionRepository.GetTransactionByIdAsync(transaction.Id, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetTransactionsInvolvingUsersAsync_ShouldReturnTransactionsWhereTheUserIsTheSender()
    {
        // Arrange
        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        var transaction = new TransactionAggregate(
            "Transaction A",
            new(100m, new("SEK")),
            userA.Id,
            [userB.Id],
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );

        // Act
        await transactionRepository.SaveAsync(transaction, CancellationToken.None);
        var result = await transactionRepository
            .GetTransactionsInvolvingUserAsync(userA.Id, CancellationToken.None)
            .ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(transaction.Id, result.Single().Id);
    }

    [TestMethod]
    public async Task GetTransactionsInvolvingUsersAsync_ShouldReturnTransactionsWhereTheUserIsARecipient()
    {
        // Arrange
        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        var transaction = new TransactionAggregate(
            "Transaction A",
            new(100m, new("SEK")),
            userA.Id,
            [userB.Id],
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );

        // Act
        await transactionRepository.SaveAsync(transaction, CancellationToken.None);
        var result = await transactionRepository
            .GetTransactionsInvolvingUserAsync(userB.Id, CancellationToken.None)
            .ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(transaction.Id, result.Single().Id);
    }

    [TestMethod]
    public async Task GetTransactionsInvolvingUsersAsync_ShouldNotReturnRemovedTransactions()
    {
        // Arrange
        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        var transaction = new TransactionAggregate(
            "Transaction A",
            new(100m, new("SEK")),
            userA.Id,
            [userB.Id],
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );
        transaction.Remove(new(2025, 08, 19, 15, 05, 00, new(00, 00, 00)));

        // Act
        await transactionRepository.SaveAsync(transaction, CancellationToken.None);
        var result = await transactionRepository
            .GetTransactionsInvolvingUserAsync(userA.Id, CancellationToken.None)
            .ToArrayAsync();

        // Assert
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public async Task SavingATransactionShouldFlushItsDomainEvents()
    {
        // Arrange
        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        var transaction = new TransactionAggregate(
            "Transaction A",
            new(100m, new("SEK")),
            userA.Id,
            [userB.Id],
            new(2025, 06, 09, 17, 22, 00, new(00, 00, 00))
        );

        // Act
        await transactionRepository.SaveAsync(transaction, CancellationToken.None);

        // Assert
        var domainEvents = transaction.FlushDomainEvents();
        Assert.IsEmpty(domainEvents);
        await Mediator.Received(1).Publish(Arg.Any<TransactionCreatedEvent>(), Arg.Any<CancellationToken>());
    }
}
