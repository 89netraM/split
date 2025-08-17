using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.AuthKeyIdTests;

[TestClass]
public sealed class ConstructorShould
{
    [TestMethod]
    public void ConstructAValidId()
    {
        // Arrange
        var id = "Anything non empty is valid";

        // Act & Assert
        // Should not throw
        _ = new AuthKeyId(id);
    }

    [TestMethod]
    public void NotConstructAnNullId()
    {
        // Arrange
        string id = null!;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AuthKeyId(id));
    }

    [TestMethod]
    public void NotConstructAnEmptyId()
    {
        // Arrange
        var id = "";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AuthKeyId(id));
    }

    [TestMethod]
    public void NotConstructAWhitespaceOnlyId()
    {
        // Arrange
        var id = " \t \n ";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AuthKeyId(id));
    }
}
