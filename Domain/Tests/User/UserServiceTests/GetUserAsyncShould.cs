using System;
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
public class GetUserAsyncShould
{
    [TestMethod]
    public async Task ReturnUser_WhenUserExists()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 06, 13, 43, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(
            new("existing-user-id"),
            "Test User",
            new("+1234567890"),
            timeProvider.GetUtcNow()
        );
        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            new InMemoryUserRepository(user)
        );

        // Act
        var result = await userService.GetUserAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result!.Id);
    }

    [TestMethod]
    public async Task ReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = new UserId("nonexistent-user-id");
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(),
            new InMemoryUserRepository()
        );

        // Act
        var result = await userService.GetUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ReturnNull_WhenUserHasBeenRemoved()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 06, 13, 43, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(
            new("existing-user-id"),
            "Test User",
            new("+1234567890"),
            timeProvider.GetUtcNow()
        );
        user.Remove(timeProvider.GetUtcNow());
        var userService = new UserService(
            new NullLogger<UserService>(),
            timeProvider,
            new InMemoryUserRepository(user)
        );

        // Act
        var result = await userService.GetUserAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }
}
