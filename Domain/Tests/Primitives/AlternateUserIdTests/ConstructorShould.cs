using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.AlternateUserIdTests;

[TestClass]
public sealed class ConstructorShould
{
    [TestMethod]
    public void ConstructAValidId()
    {
        // Arrange
        var type = "Anything non empty is valid";
        var id = "Anything non empty is valid";

        // Act & Assert
        // Should not throw
        _ = new AlternateUserId(type, id);
    }

    [TestMethod]
    public void NotConstructAnNullId()
    {
        // Arrange
        var type = "type";
        string id = null!;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AlternateUserId(type, id));
    }

    [TestMethod]
    public void NotConstructAnEmptyId()
    {
        // Arrange
        var type = "type";
        var id = "";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AlternateUserId(type, id));
    }

    [TestMethod]
    public void NotConstructAWhitespaceOnlyId()
    {
        // Arrange
        var type = "type";
        var id = " \t \n ";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AlternateUserId(type, id));
    }

    [TestMethod]
    public void NotConstructAnNullType()
    {
        // Arrange
        string type = null!;
        var id = "id";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AlternateUserId(type, id));
    }

    [TestMethod]
    public void NotConstructAnEmptyType()
    {
        // Arrange
        var type = "";
        var id = "id";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AlternateUserId(type, id));
    }

    [TestMethod]
    public void NotConstructAWhitespaceOnlyType()
    {
        // Arrange
        var type = " \t \n ";
        var id = "id";

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => new AlternateUserId(type, id));
    }
}
