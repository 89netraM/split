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

namespace Split.Domain.Tests.User.Events.RemoveFriendshipRequestTests;

[TestClass]
public class RemoveFriendshipRequestTests
{
    [TestMethod]
    public async Task SuccessfullyRemoveAFriendshipAsync()
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
        var removeFriendshipRequestHandler = new RemoveFriendshipRequestHandler(
            new UserService(new NullLogger<UserService>(), timeProvider, userRepository)
        );

        // Act
        var response = await removeFriendshipRequestHandler.Handle(new(userA.Id, userB.Id), CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(userA.Friendships.Any(f => f.FriendId == userB.Id));
        Assert.IsFalse(userB.Friendships.Any(f => f.FriendId == userA.Id));
    }
}
