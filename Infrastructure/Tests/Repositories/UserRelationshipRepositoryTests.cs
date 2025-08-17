using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Transaction;
using Split.Domain.User;

namespace Split.Infrastructure.Tests.Repositories;

[TestClass]
public class UserRelationshipRepositoryTests : PostgresTestBase
{
    [TestMethod]
    public async Task GetRelatedUsersByUserIdShould_ReturnUsersWithTransactionsToAndFromUserId()
    {
        // Arrange
        var userRepository = Services.GetRequiredService<IUserRepository>();
        var userA = new UserAggregate(
            new("user-A"),
            "User A",
            new("+1234567890"),
            new(2025, 06, 20, 05, 16, 00, new(00, 00, 00))
        );
        await userRepository.SaveAsync(userA, CancellationToken.None);
        var userB = new UserAggregate(
            new("user-B"),
            "User B",
            new("+9876543210"),
            new(2025, 06, 20, 05, 16, 00, new(00, 00, 00))
        );
        await userRepository.SaveAsync(userB, CancellationToken.None);
        var userC = new UserAggregate(
            new("user-C"),
            "User C",
            new("+9182736450"),
            new(2025, 06, 20, 05, 16, 00, new(00, 00, 00))
        );
        await userRepository.SaveAsync(userC, CancellationToken.None);

        var transactionRepository = Services.GetRequiredService<ITransactionRepository>();
        await transactionRepository.SaveAsync(
            new(
                "Oldest Transaction",
                new(100.0m, new("SEK")),
                userA.Id,
                [userC.Id],
                new(2025, 06, 20, 05, 21, 00, new(00, 00, 00))
            ),
            CancellationToken.None
        );
        await transactionRepository.SaveAsync(
            new(
                "Latest Transaction",
                new(100.0m, new("SEK")),
                userB.Id,
                [userA.Id],
                new(2025, 06, 20, 05, 22, 00, new(00, 00, 00))
            ),
            CancellationToken.None
        );

        var userRelationshipRepository = Services.GetRequiredService<IUserRelationshipRepository>();

        // Act
        var relatedUsers = await userRelationshipRepository
            .GetRelatedUsersAsync(userA.Id, CancellationToken.None)
            .ToArrayAsync();

        // Assert
        Assert.AreEqual(2, relatedUsers.Length);
        Assert.Contains(userB.Id, relatedUsers.Select(u => u.Id));
        Assert.Contains(userC.Id, relatedUsers.Select(u => u.Id));
    }
}
