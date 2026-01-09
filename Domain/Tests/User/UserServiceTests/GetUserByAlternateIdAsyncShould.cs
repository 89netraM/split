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
public class GetUserByAlternateIdAsyncShould
{
    [TestMethod]
    public async Task ReturnUser_WhenUserExists()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2026, 01, 09, 09, 02, 00, new(00, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(
            new("existing-user-id"),
            "Test User",
            new("+1234567890"),
            timeProvider.GetUtcNow()
        );
        user.AlternateIds.Add(new("test-type", "alternate-id"));
        var userRepository = new InMemoryUserRepository(user);
        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            userRepository,
            new InMemoryUserRelationshipRepository()
        );

        // Act
        var result = await userService.GetUserByAlternateIdAsync(user.AlternateIds.First(), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result!.Id);
    }

    [TestMethod]
    public async Task ReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var alternateUserId = new AlternateUserId("test-type", "nonexistent-alternate-id");
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(),
            new InMemoryUserRepository(),
            new InMemoryUserRelationshipRepository()
        );

        // Act
        var result = await userService.GetUserByAlternateIdAsync(alternateUserId, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ReturnNull_WhenUserHasBeenRemoved()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2026, 01, 09, 09, 02, 00, new(00, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(
            new("existing-user-id"),
            "Test User",
            new("+1234567890"),
            timeProvider.GetUtcNow()
        );
        user.AlternateIds.Add(new("test-type", "alternate-id"));
        user.Remove(timeProvider.GetUtcNow());
        var userRepository = new InMemoryUserRepository(user);
        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            userRepository,
            new InMemoryUserRelationshipRepository()
        );

        // Act
        var result = await userService.GetUserByAlternateIdAsync(user.AlternateIds.First(), CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }
}
