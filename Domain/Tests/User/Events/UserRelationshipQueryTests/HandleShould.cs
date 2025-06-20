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

namespace Split.Domain.Tests.User.Events.UserRelationshipQueryTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task ReturnTheUsersThatTheRepositoryReturns()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 06, 20, 09, 58, 00, new(00, 00, 00)))
        {
            AutoAdvanceAmount = TimeSpan.FromMinutes(1),
        };
        var userA = new UserAggregate(new("user-A"), "Test A", new("+1234567890"), timeProvider.GetUtcNow());
        var userB = new UserAggregate(new("user-B"), "Test B", new("+1892563478"), timeProvider.GetUtcNow());
        var userC = new UserAggregate(new("user-C"), "Test C", new("+1983453533"), timeProvider.GetUtcNow());
        var userRepository = new InMemoryUserRepository(userA, userB, userC);

        var userRelationshipRepository = new InMemoryUserRelationshipRepository(new() { [userA.Id] = [userB, userC] });

        var userRelationshipQueryHandler = new UserRelationshipQueryHandler(
            new(new NullLogger<UserService>(), timeProvider, userRepository, userRelationshipRepository)
        );

        // Act
        var result = await userRelationshipQueryHandler.Handle(new(userA.Id), CancellationToken.None).ToArrayAsync();

        // Assert
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(userB.Id, result[0].User.Id);
        Assert.AreEqual(userC.Id, result[1].User.Id);
    }
}
