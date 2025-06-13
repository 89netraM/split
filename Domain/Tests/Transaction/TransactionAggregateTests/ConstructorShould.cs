using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Domain.Transaction;

namespace Split.Domain.Tests.Transaction.TransactionAggregateTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    public void ThrowNoRecipientsException_WhenRecipientIdsIsEmpty()
    {
        // Arrange
        var description = "Test Transaction";
        var amount = new Money(100, new("SEK"));
        var senderId = new UserId("sender-id");
        var recipientIds = new List<UserId>();

        // Act
        var action = () =>
            new TransactionAggregate(
                description,
                amount,
                senderId,
                recipientIds,
                new(2025, 06, 13, 17, 00, 00, new(00, 00, 00))
            );

        // Assert
        Assert.ThrowsExactly<NoRecipientsException>(action);
    }
}
