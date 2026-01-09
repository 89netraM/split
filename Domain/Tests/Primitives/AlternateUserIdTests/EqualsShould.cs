using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;

namespace Split.Domain.Tests.Primitives.AlternateUserIdTests;

[TestClass]
public sealed class EqualsShould
{
    [TestMethod]
    public void ReturnTrueForTheSameObject()
    {
        // Arrange
        var alternateUserId = new AlternateUserId("type", "id");

        // Act
        var result = alternateUserId.Equals(alternateUserId);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ReturnTrueForAnEquivalentObject()
    {
        // Arrange
        var alternateUserId1 = new AlternateUserId("type", "id");
        var alternateUserId2 = new AlternateUserId("type", "id");

        // Act
        var result = alternateUserId1.Equals(alternateUserId2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void NotReturnTrueForTheSameIdOfDifferentTypes()
    {
        // Arrange
        var alternateUserId1 = new AlternateUserId("type1", "id");
        var alternateUserId2 = new AlternateUserId("type2", "id");

        // Act
        var result = alternateUserId1.Equals(alternateUserId2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void NotReturnTrueForTheSameTypeWithDifferentIds()
    {
        // Arrange
        var alternateUserId1 = new AlternateUserId("type", "id1");
        var alternateUserId2 = new AlternateUserId("type", "id2");

        // Act
        var result = alternateUserId1.Equals(alternateUserId2);

        // Assert
        Assert.IsFalse(result);
    }
}
