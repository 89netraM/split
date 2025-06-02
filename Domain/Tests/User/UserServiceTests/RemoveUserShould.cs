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
public class RemoveUserShould
{
    [TestMethod]
    public async Task SuccessfullyRemoveAUser()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 01, 27, 00, new(02, 00, 00)));
        var user = new UserAggregate(
            new("8d09b539-6019-43d5-a133-18372d77576e"),
            "A. N. Other",
            new("1234567890"),
            timeProvider.GetUtcNow()
        );
        var userRepository = new InMemoryUserRepository(user);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act
        await userService.RemoveUserAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.IsNotNull(user.RemovedAt);
    }

    [TestMethod]
    public async Task BeIdempotent_ByNotFailingWhenTheUserDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 01, 27, 00, new(02, 00, 00)));
        var userId = new UserId("063db591-512f-4b5e-8e20-e78950419ec2");
        var userRepository = new InMemoryUserRepository();
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act & Assert
        await userService.RemoveUserAsync(userId, CancellationToken.None);
    }

    [TestMethod]
    public async Task BeIdempotent_ByNotFailingWhenTheUserIsAlreadyRemoved()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 01, 27, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(
            new("8371ebfb-70a3-48a7-ae64-afc80c37d955"),
            "A. N. Other",
            new("1234567890"),
            timeProvider.GetUtcNow()
        );
        var removedAt = timeProvider.GetUtcNow();
        user.Remove(removedAt);
        var userRepository = new InMemoryUserRepository(user);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act
        await userService.RemoveUserAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.AreEqual(removedAt, user.RemovedAt);
    }
}
