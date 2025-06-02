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
public class CreateUserShould
{
    [TestMethod]
    public async Task SuccessfullyCreateAUser()
    {
        // Arrange
        var name = "A. N. Other";
        var phoneNumber = new PhoneNumber("1234567890");
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 05, 31, 00, 49, 00, new(02, 00, 00))),
            new InMemoryUserRepository()
        );

        // Act
        var user = await userService.CreateUserAsync(
            new("ea77aaef-e57a-4d30-8e62-acb514f7b1be"),
            name,
            phoneNumber,
            CancellationToken.None
        );

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
        var existingUser = new UserAggregate(
            new("a615cde3-c61e-47f7-9093-19bef8085edf"),
            "A. N. Other",
            new("1234567890"),
            timeProvider.GetUtcNow()
        );
        var userRepository = new InMemoryUserRepository(existingUser);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act
        var createdUser = await userService.CreateUserAsync(
            existingUser.Id,
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
    public async Task ThrowIfIdAlreadyExistsWithDifferentName()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 00, 49, 00, new(02, 00, 00)));
        var existingUser = new UserAggregate(
            new("97b0f29d-60f9-44ea-af47-65c5a627536e"),
            "Existing User",
            new PhoneNumber("1234567890"),
            timeProvider.GetUtcNow()
        );
        var userRepository = new InMemoryUserRepository(existingUser);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UserAlreadyExistsException>(() =>
            userService.CreateUserAsync(existingUser.Id, "New User", existingUser.PhoneNumber, CancellationToken.None)
        );
    }

    [TestMethod]
    public async Task ThrowIfIdAlreadyExistsWithDifferentPhoneNumber()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 00, 49, 00, new(02, 00, 00)));
        var existingUser = new UserAggregate(
            new("97b0f29d-60f9-44ea-af47-65c5a627536e"),
            "A. N. Other",
            new PhoneNumber("1234567890"),
            timeProvider.GetUtcNow()
        );
        var userRepository = new InMemoryUserRepository(existingUser);
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<UserAlreadyExistsException>(() =>
            userService.CreateUserAsync(existingUser.Id, existingUser.Name, new("0987654321"), CancellationToken.None)
        );
    }

    [TestMethod]
    public async Task ThrowIfAnotherUserHasTheSamePhoneNumber()
    {
        // Arrange
        var id = new UserId("f35a40a1-cd24-46d0-a776-73f7111ed3e8");
        var name = "A. N. Other";
        var phoneNumber = new PhoneNumber("1234567890");
        var timeProvider = new FakeTimeProvider(new(2025, 05, 31, 00, 49, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userRepository = new InMemoryUserRepository(
            new UserAggregate(new("user-id"), "Existing User", phoneNumber, timeProvider.GetUtcNow())
        );
        var userService = new UserService(new NullLogger<UserService>(), timeProvider, userRepository);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PhoneNumberInUseException>(() =>
            userService.CreateUserAsync(id, name, phoneNumber, CancellationToken.None)
        );
    }
}
