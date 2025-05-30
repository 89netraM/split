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
public class CreateUserShould
{
    [TestMethod]
    public async Task SuccessfullyCreateAUser()
    {
        // Arrange
        var name = "A. N. Other";
        var phoneNumber = new PhoneNumber("1234567890");
        var userService = new UserService(
            Substitute.For<ILogger<UserService>>(),
            new FakeTimeProvider(new(2025, 05, 31, 00, 49, 00, new(02, 00, 00))),
            Substitute.For<IUserRepository>()
        );

        // Act
        var user = await userService.CreateUserAsync(name, phoneNumber, CancellationToken.None);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual(name, user.Name);
        Assert.AreEqual(phoneNumber, user.PhoneNumber);
    }

    [TestMethod]
    public async Task BeIdempotent()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 00, 49, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var existingUser = new UserAggregate("A. N. Other", new("1234567890"), timeProvider.GetUtcNow());
        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetUserByPhoneNumberAsync(existingUser.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        var userService = new UserService(Substitute.For<ILogger<UserService>>(), timeProvider, userRepository);

        // Act
        var createdUser = await userService.CreateUserAsync(
            existingUser.Name,
            existingUser.PhoneNumber,
            CancellationToken.None
        );

        // Assert
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(existingUser.Id, createdUser.Id);
        Assert.AreEqual(existingUser.Name, createdUser.Name);
        Assert.AreEqual(existingUser.PhoneNumber, createdUser.PhoneNumber);
        Assert.AreEqual(existingUser.CreatedAt, createdUser.CreatedAt);
    }

    [TestMethod]
    public async Task ThrowIfPhoneNumberAlreadyExistsWithDifferentName()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 00, 49, 00, new(02, 00, 00)));
        var existingUser = new UserAggregate("Existing User", new PhoneNumber("1234567890"), timeProvider.GetUtcNow());
        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .GetUserByPhoneNumberAsync(existingUser.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns(existingUser);
        var userService = new UserService(Substitute.For<ILogger<UserService>>(), timeProvider, userRepository);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PhoneNumberAlreadyExistsException>(() =>
            userService.CreateUserAsync("New User", existingUser.PhoneNumber, CancellationToken.None)
        );
    }
}
