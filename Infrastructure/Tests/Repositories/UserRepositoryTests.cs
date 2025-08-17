using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Infrastructure.Tests.Repositories;

[TestClass]
public class UserRepositoryTests : PostgresTestBase
{
    [TestMethod]
    public async Task NothingShouldBeRetrievableWhenTheDatabaseIsEmpty()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var userId = new UserId("nonexistent-user-id");

        // Act
        var result = await userRepository.GetUserByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ASavedUserShouldBeRetrievableById()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var user = new UserAggregate(
            new("user-id"),
            "Test User",
            new("+9123456789"),
            new(2025, 06, 09, 16, 25, 00, new(00, 00, 00))
        );

        // Act
        await userRepository.SaveAsync(user, CancellationToken.None);
        var result = await userRepository.GetUserByIdAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result.Id);
        Assert.AreEqual(user.Name, result.Name);
        Assert.AreEqual(user.PhoneNumber, result.PhoneNumber);
        Assert.AreEqual(user.CreatedAt, result.CreatedAt);
        Assert.AreEqual(user.RemovedAt, result.RemovedAt);
    }

    [TestMethod]
    public async Task ASavedUserShouldBeRetrievableByPhoneNumber()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var user = new UserAggregate(
            new("user-id"),
            "Test User",
            new("+9123456789"),
            new(2025, 06, 09, 16, 25, 00, new(00, 00, 00))
        );

        // Act
        await userRepository.SaveAsync(user, CancellationToken.None);
        var result = await userRepository.GetUserByPhoneNumberAsync(user.PhoneNumber, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result.Id);
        Assert.AreEqual(user.Name, result.Name);
        Assert.AreEqual(user.PhoneNumber, result.PhoneNumber);
        Assert.AreEqual(user.CreatedAt, result.CreatedAt);
        Assert.AreEqual(user.RemovedAt, result.RemovedAt);
    }

    [TestMethod]
    public async Task SavingAUserShouldFlushItsDomainEvents()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var user = new UserAggregate(
            new("user-id"),
            "Test User",
            new("+9123456789"),
            new(2025, 06, 09, 16, 25, 00, new(00, 00, 00))
        );

        // Act
        await userRepository.SaveAsync(user, CancellationToken.None);

        // Assert
        var domainEvents = user.FlushDomainEvents();
        Assert.IsEmpty(domainEvents);
        await Mediator.Received(1).Publish(Arg.Any<UserCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ASavedUserWithAnAuthKeyShouldHaveItsAuthKeyWhenRetrievedById()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var user = new UserAggregate(
            new("user-id"),
            "Test User",
            new("+9123456789"),
            new(2025, 08, 15, 14, 32, 00, new(00, 00, 00))
        );
        user.AddAuthKey(new("auth-key-id"), [0x12], 2);
        var authKey = user.AuthKeys.Single();

        // Act
        await userRepository.SaveAsync(user, CancellationToken.None);
        var result = await userRepository.GetUserByIdAsync(user.Id, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.ContainsSingle(result.AuthKeys);
        var retrievedAuthKey = result.AuthKeys.Single();
        Assert.AreEqual(authKey.Id, retrievedAuthKey.Id);
        Assert.AreEqual(authKey.Key, retrievedAuthKey.Key);
        Assert.AreEqual(authKey.SignCount, retrievedAuthKey.SignCount);
    }

    [TestMethod]
    public async Task DoesAuthKeyExistShouldReturnFalseWhenNoAuthKeysExist()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var user = new UserAggregate(
            new("user-id"),
            "Test User",
            new("+9123456789"),
            new(2025, 08, 15, 14, 32, 00, new(00, 00, 00))
        );

        // Act
        await userRepository.SaveAsync(user, CancellationToken.None);
        var result = await userRepository.DoesAuthKeyIdExist(new("auth-key-id"), CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task DoesAuthKeyExistShouldReturnFalseWhenTheAuthKeyDoesNotExistButOtherDo()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var user = new UserAggregate(
            new("user-id"),
            "Test User",
            new("+9123456789"),
            new(2025, 08, 15, 14, 32, 00, new(00, 00, 00))
        );
        user.AddAuthKey(new("auth-key-id"), [0x12], 2);

        // Act
        await userRepository.SaveAsync(user, CancellationToken.None);
        var result = await userRepository.DoesAuthKeyIdExist(new("another-auth-key-id"), CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task DoesAuthKeyExistShouldReturnTrueWhenTheAuthKeyExists()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var user = new UserAggregate(
            new("user-id"),
            "Test User",
            new("+9123456789"),
            new(2025, 08, 15, 14, 32, 00, new(00, 00, 00))
        );
        user.AddAuthKey(new("auth-key-id"), [0x12], 2);

        // Act
        await userRepository.SaveAsync(user, CancellationToken.None);
        var result = await userRepository.DoesAuthKeyIdExist(new("auth-key-id"), CancellationToken.None);

        // Assert
        Assert.IsTrue(result);
    }
}
