using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.AlternateUserQueryTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task ReturnUser_WhenUserExists()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2026, 01, 07, 10, 23, 00, new(00, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var user = new UserAggregate(
            new("existing-user-id"),
            "Test User",
            new("+1234567890"),
            timeProvider.GetUtcNow()
        );
        user.AlternateIds.Add(new("test-type", "existing-alternate-id"));
        var userRepository = new InMemoryUserRepository(user);
        var handler = new AlternateUserQueryHandler(
            new(new NullLogger<UserService>(), timeProvider, userRepository, new InMemoryUserRelationshipRepository())
        );

        // Act
        var result = await handler.Handle(new(user.AlternateIds.First()), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.User);
        Assert.AreEqual(user.Id, result.User.Id);
    }
}
