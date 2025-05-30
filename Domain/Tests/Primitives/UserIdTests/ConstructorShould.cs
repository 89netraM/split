using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.UserIdTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    public void ConstructValidUserId()
    {
        // Arrange
        var userId = "c973be76-8b35-49e6-8fc0-24f2b968a0d0";

        // Act & Assert
        // Should not throw an exception for a valid UserId
        _ = new UserId(new(userId));
    }

    [TestMethod]
    public void ThrowExceptionForEmptyUserId()
    {
        // Arrange
        var emptyUserId = Guid.Empty;

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new UserId(emptyUserId));
    }
}
