using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;

namespace Split.Domain.Tests.User.UserServiceTests;

[TestClass]
public class GetFriendsAsyncShould
{
    [TestMethod]
    public async Task SuccessfullyReturnAllFriendsOfAUserAsync()
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

        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            new InMemoryUserRepository(user, friend1, friend2)
        );

        // Act
        var result = await userService.GetFriendsAsync(user.Id, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(result.Any(friend => friend.Id == friend1.Id));
        Assert.IsTrue(result.Any(friend => friend.Id == friend2.Id));
    }

    [TestMethod]
    public async Task NotReturnFriends_WhenTheirFriendshipHasBeenRemoved()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 05, 17, 50, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(new("user"), "User", new("0123456789"), timeProvider.GetUtcNow());
        var friend = new UserAggregate(new("friend"), "Friend", new("0987654321"), timeProvider.GetUtcNow());
        var formerFriend = new UserAggregate(
            new("formerFriend"),
            "Former Friend",
            new("0147258369"),
            timeProvider.GetUtcNow()
        );
        user.CreateFriendship(friend, timeProvider.GetUtcNow());
        user.CreateFriendship(formerFriend, timeProvider.GetUtcNow());
        user.RemoveFriendship(formerFriend, timeProvider.GetUtcNow());

        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            new InMemoryUserRepository(user, friend, formerFriend)
        );

        // Act
        var result = await userService.GetFriendsAsync(user.Id, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(1, result.Length);
        Assert.IsTrue(result.Any(friend => friend.Id == friend.Id));
        Assert.IsFalse(result.Any(friend => friend.Id == formerFriend.Id));
    }

    [TestMethod]
    public async Task NotReturnAnyFriends_ForUsersThatDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 05, 17, 50, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(new("user"), "User", new("0123456789"), timeProvider.GetUtcNow());
        var friend = new UserAggregate(new("friend"), "Friend", new("0987654321"), timeProvider.GetUtcNow());
        user.CreateFriendship(friend, timeProvider.GetUtcNow());

        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            new InMemoryUserRepository(user, friend)
        );

        // Act
        var result = await userService.GetFriendsAsync(new("non-existent-user"), CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public async Task NotReturnFriends_ThatDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 05, 17, 50, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(new("user"), "User", new("0123456789"), timeProvider.GetUtcNow());
        var friend = new UserAggregate(new("friend"), "Friend", new("0987654321"), timeProvider.GetUtcNow());
        user.CreateFriendship(friend, timeProvider.GetUtcNow());

        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            new InMemoryUserRepository(user) // Intentionally not including `friend` to simulate a missing friend
        );

        // Act
        var result = await userService.GetFriendsAsync(user.Id, CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(0, result.Length);
    }
}
