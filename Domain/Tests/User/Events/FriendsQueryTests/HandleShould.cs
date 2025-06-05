using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.FriendsQueryTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task SuccessfullyReturnAllFriendsOfAUser()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 05, 17, 50, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(new("user"), "User", new("0123456789"), timeProvider.GetUtcNow());
        var friend1 = new UserAggregate(new("friend1"), "Friend 1", new("0987654321"), timeProvider.GetUtcNow());
        var friend2 = new UserAggregate(new("friend2"), "Friend 2", new("0147258369"), timeProvider.GetUtcNow());
        user.CreateFriendship(friend1, timeProvider.GetUtcNow());
        user.CreateFriendship(friend2, timeProvider.GetUtcNow());

        var handler = new FriendsQueryHandler(
            new UserService(
                new NullLogger<UserService>(),
                timeProvider,
                new InMemoryUserRepository(user, friend1, friend2)
            )
        );

        // Act
        var result = await handler.Handle(new(user.Id), CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(result.Any(r => r.Friend.Id == friend1.Id));
        Assert.IsTrue(result.Any(r => r.Friend.Id == friend2.Id));
    }
}
