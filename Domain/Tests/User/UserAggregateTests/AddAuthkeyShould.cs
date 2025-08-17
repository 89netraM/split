using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.UserAggregateTests;

[TestClass]
public sealed class AddAuthKeyShould
{
    [TestMethod]
    public void AddAnAuthKeyEntity()
    {
        // Arrange
        var user = new UserAggregate(
            new("User-ID"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 13, 13, 05, 00, new(00, 00, 00))
        );
        var id = new AuthKeyId("AuthKey-ID");
        byte[] key = [0, 1, 2, 3, 4, 5];

        // Act
        user.AddAuthKey(id, key, counter: 0);

        // Assert
        Assert.IsTrue(user.AuthKeys.Any(k => k.Id == id));
    }

    [TestMethod]
    public void AddAnAuthKeyEntityWithZeroSignatures()
    {
        // Arrange
        var user = new UserAggregate(
            new("User-ID"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 13, 13, 05, 00, new(00, 00, 00))
        );
        var id = new AuthKeyId("AuthKey-ID");
        byte[] key = [0, 1, 2, 3, 4, 5];

        // Act
        user.AddAuthKey(id, key, counter: 0);

        // Assert
        Assert.AreEqual(0u, user.AuthKeys.Single(k => k.Id == id).SignCount);
    }

    [TestMethod]
    public void ProduceADomainEventForTheAddedAuthKey()
    {
        // Arrange
        var user = new UserAggregate(
            new("User-ID"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 13, 13, 05, 00, new(00, 00, 00))
        );
        user.FlushDomainEvents();
        var id = new AuthKeyId("AuthKey-ID");
        byte[] key = [0, 1, 2, 3, 4, 5];

        // Act
        user.AddAuthKey(id, key, counter: 0);

        // Assert
        var events = user.FlushDomainEvents();
        Assert.ContainsSingle(events);
        var @event = events.Single();
        Assert.IsInstanceOfType<UserAuthKeyAddedEvent>(@event);
        Assert.AreEqual(id, ((UserAuthKeyAddedEvent)@event).KeyId);
    }
}
