using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.User;

namespace Split.Domain.Tests.User.AuthKeyEntityTests;

[TestClass]
public sealed class IncreaseSignCountShould
{
    [TestMethod]
    public void IncreaseTheSignCountByOne()
    {
        // Arrange
        var authKey = new AuthKeyEntity(new("auth-key-id"), [0x00], 0);

        // Act
        authKey.IncreaseSignCount(1);

        // Assert
        Assert.AreEqual(1u, authKey.SignCount);
    }

    [TestMethod]
    public void ThrowWhenDecreasingSignCount()
    {
        // Arrange
        var authKey = new AuthKeyEntity(new("auth-key-id"), [0x00], 1);

        // Act
        void act() => authKey.IncreaseSignCount(0);

        // Assert
        Assert.ThrowsExactly<SignCountIncreaseException>(act);
    }

    [TestMethod]
    public void ThrowWhenIncreasingByMoreThanOne()
    {
        // Arrange
        var authKey = new AuthKeyEntity(new("auth-key-id"), [0x00], 1);

        // Act
        void act() => authKey.IncreaseSignCount(3);

        // Assert
        Assert.ThrowsExactly<SignCountIncreaseException>(act);
    }
}
