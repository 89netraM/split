using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.UserQueryTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task ReturnUser_WhenUserExists()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 06, 13, 43, 00, new(02, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(new("existing-user-id"), "Test User", new("1234567890"), timeProvider.GetUtcNow());
        var handler = new UserQueryHandler(
            new(new NullLogger<UserService>(), timeProvider, new InMemoryUserRepository(user))
        );

        // Act
        var result = await handler.Handle(new(user.Id), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result!.User);
        Assert.AreEqual(user.Id, result!.User!.Id);
    }
}
