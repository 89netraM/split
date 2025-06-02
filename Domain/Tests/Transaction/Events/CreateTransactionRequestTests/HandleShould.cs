using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Domain.Tests.TestCommon;
using Split.Domain.Transaction;
using Split.Domain.Transaction.Events;
using Split.Domain.User;
using Split.Utilities;

namespace Split.Domain.Tests.Transaction.Events.CreateTransactionRequestTests;

[TestClass]
public class HandleShould
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
        var sender = new UserAggregate(new("user-sender"), "Sender", new("0123456789"), timeProvider.GetUtcNow());
        var recipient = new UserAggregate(
            new("user-recipient"),
            "Recipient",
            new("9876543210"),
            timeProvider.GetUtcNow()
        );
        sender.CreateFriendship(recipient, timeProvider.GetUtcNow());

        var userRepository = new InMemoryUserRepository(sender, recipient);
        var recipientIds = new NonEmptyImmutableSet<UserId>(sender.Id, recipient.Id);
        var handler = new CreateTransactionRequestHandler(
            new TransactionService(
                new NullLogger<TransactionService>(),
                timeProvider,
                new InMemoryTransactionRepository(),
                userRepository
            )
        );

        // Act
        var response = await handler.Handle(
            new CreateTransactionRequest(amount, description, sender.Id, recipientIds),
            CancellationToken.None
        );

        // Assert
        Assert.IsNotNull(response);
    }
}
