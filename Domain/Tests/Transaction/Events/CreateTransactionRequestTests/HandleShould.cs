using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
using Split.Domain.Transaction;
using Split.Domain.Transaction.Events;
using Split.Utilities;

namespace Split.Domain.Tests.Transaction.Events.CreateTransactionRequestTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task SuccessfullyCreateATransaction()
    {
        // Arrange
        var amount = new Money(250, new("SEK"));
        var description = "Lunch";
        var senderId = new UserId("2eeab634-45a8-4b3a-840a-084197d687fa");
        var recipientIds = new NonEmptyImmutableSet<UserId>(
            senderId,
            new UserId("2eeab634-45a8-4b3a-840a-084197d687fc")
        );
        var handler = new CreateTransactionRequestHandler(
            new TransactionService(
                new NullLogger<TransactionService>(),
                new FakeTimeProvider(),
                Substitute.For<ITransactionRepository>()
            )
        );

        // Act
        var response = await handler.Handle(
            new CreateTransactionRequest(amount, description, senderId, recipientIds),
            CancellationToken.None
        );

        // Assert
        Assert.IsNotNull(response);
    }
}
