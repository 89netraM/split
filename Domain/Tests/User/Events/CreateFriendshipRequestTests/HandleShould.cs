using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.CreateFriendshipRequestTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task SuccessfullyCreateAFriendship()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 01, 19, 52, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserAggregate(new("UserA"), "User A", new("0123456789"), timeProvider.GetUtcNow());
        var userB = new UserAggregate(new("UserB"), "User B", new("9876543210"), timeProvider.GetUtcNow());

        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserByIdAsync(userA.Id, Arg.Any<CancellationToken>()).Returns(userA);
        userRepository.GetUserByIdAsync(userB.Id, Arg.Any<CancellationToken>()).Returns(userB);
        var createFriendshipRequestHandler = new CreateFriendshipRequestHandler(
            new UserService(new NullLogger<UserService>(), timeProvider, userRepository)
        );

        // Act
        var response = await createFriendshipRequestHandler.Handle(new(userA.Id, userB.Id), CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(userA.Friendships.Any(f => f.FriendId == userB.Id));
        Assert.IsTrue(userB.Friendships.Any(f => f.FriendId == userA.Id));
    }
}
