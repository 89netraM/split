using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
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
        var user = new UserAggregate("A. N. Other", new("1234567890"), timeProvider.GetUtcNow());
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var userService = new UserService(Substitute.For<ILogger<UserService>>(), timeProvider, userRepository);

        // Act
        await userService.RemoveUserAsync(user.Id, CancellationToken.None);

        // Assert
        await userRepository.Received(1).SaveAsync(user, Arg.Any<CancellationToken>());
        Assert.IsNotNull(user.RemovedAt);
    }

    [TestMethod]
    public async Task BeIdempotent_ByNotFailingWhenTheUserDoesNotExist()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 01, 27, 00, new(02, 00, 00)));
        var userId = new UserId(Guid.NewGuid());
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserAggregate?)null);
        var userService = new UserService(Substitute.For<ILogger<UserService>>(), timeProvider, userRepository);

        // Act
        await userService.RemoveUserAsync(userId, CancellationToken.None);

        // Assert
        await userRepository.DidNotReceive().SaveAsync(Arg.Any<UserAggregate>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task BeIdempotent_ByNotFailingWhenTheUserIsAlreadyRemoved()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 01, 27, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate("A. N. Other", new("1234567890"), timeProvider.GetUtcNow());
        var removedAt = timeProvider.GetUtcNow();
        user.Remove(removedAt);
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetUserByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        var userService = new UserService(Substitute.For<ILogger<UserService>>(), timeProvider, userRepository);

        // Act
        await userService.RemoveUserAsync(user.Id, CancellationToken.None);

        // Assert
        await userRepository.Received(1).SaveAsync(user, Arg.Any<CancellationToken>());
        Assert.AreEqual(removedAt, user.RemovedAt);
    }
}
