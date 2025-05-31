using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.UserIdTests;

[TestClass]
public class ConstructorShould
{
    [TestMethod]
    [DataRow("c973be76-8b35-49e6-8fc0-24f2b968a0d0")]
    [DataRow("12345678-1234-1234-1234-123456789012")]
    [DataRow("00000000-0000-0000-0000-000000000000")]
    [DataRow("a-n-other")]
    public void ConstructValidUserId(string userId)
    {
        // Act & Assert
        // Should not throw an exception for a valid UserId
        _ = new UserId(userId);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("    ")]
    [DataRow("\t")]
    public void ThrowExceptionForEmptyUserId(string userId)
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => new UserId(userId));
    }
}
