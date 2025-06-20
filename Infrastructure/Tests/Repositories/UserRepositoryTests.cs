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
}
