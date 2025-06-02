using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;

namespace Split.Domain.Tests.User.UserServiceTests;

[TestClass]
public class RemoveFriendshipShould
{
    [TestMethod]
    public async Task SuccessfullyRemoveAFriendship()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 20, 47, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserAggregate(new("UserA"), "User A", new("0123456789"), timeProvider.GetUtcNow());
        var userB = new UserAggregate(new("UserB"), "User B", new("9876543210"), timeProvider.GetUtcNow());

        userA.CreateFriendship(userB, timeProvider.GetUtcNow());

        var userRepository = new InMemoryUserRepository(userA, userB);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act
        await userService.RemoveFriendshipAsync(userA.Id, userB.Id, CancellationToken.None);

        // Assert
        Assert.IsFalse(userA.Friendships.Any(f => f.FriendId == userB.Id));
        Assert.IsFalse(userB.Friendships.Any(f => f.FriendId == userA.Id));
    }

    [TestMethod]
    public async Task NotDoAnything_ForTwoUsersWhoAreNotFriends()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 20, 47, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserAggregate(new("UserA"), "User A", new("0123456789"), timeProvider.GetUtcNow());
        var userB = new UserAggregate(new("UserB"), "User B", new("9876543210"), timeProvider.GetUtcNow());

        var userRepository = new InMemoryUserRepository(userA, userB);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act
        await userService.RemoveFriendshipAsync(userA.Id, userB.Id, CancellationToken.None);

        // Assert
        Assert.IsFalse(userA.Friendships.Any(f => f.FriendId == userB.Id));
        Assert.IsFalse(userB.Friendships.Any(f => f.FriendId == userA.Id));
    }

    [TestMethod]
    public async Task BeIdempotent()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 20, 47, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserAggregate(new("UserA"), "User A", new("0123456789"), timeProvider.GetUtcNow());
        var userB = new UserAggregate(new("UserB"), "User B", new("9876543210"), timeProvider.GetUtcNow());

        userA.CreateFriendship(userB, timeProvider.GetUtcNow());
        userA.RemoveFriendship(userB, timeProvider.GetUtcNow());

        var userRepository = new InMemoryUserRepository(userA, userB);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act
        await userService.RemoveFriendshipAsync(userA.Id, userB.Id, CancellationToken.None);

        // Assert
        Assert.IsFalse(userA.Friendships.Any(f => f.FriendId == userB.Id));
        Assert.IsFalse(userB.Friendships.Any(f => f.FriendId == userA.Id));
    }

    [TestMethod]
    public async Task NotDoAnything_IfOneOfTheFriendsDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 19, 52, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserAggregate(new("UserA"), "User A", new("0123456789"), timeProvider.GetUtcNow());
        var userBId = new UserId("UserB");

        var userRepository = new InMemoryUserRepository(userA);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act & Assert
        await userService.RemoveFriendshipAsync(userA.Id, userBId, CancellationToken.None);
    }
}
