using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.TransactionIdTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    public void ConstructValidUserId()
    {
        // Arrange
        var validTransactionId = Guid.CreateVersion7();

        // Act & Assert
        // Should not throw an exception for a valid TransactionId
        _ = new TransactionId(validTransactionId);
    }

    [TestMethod]
    public void ThrowExceptionForEmptyTransactionId()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new TransactionId(Guid.Empty));
    }

    [TestMethod]
    public void ThrowExceptionForVersion4TransactionId()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new TransactionId(Guid.NewGuid()));
    }
}
