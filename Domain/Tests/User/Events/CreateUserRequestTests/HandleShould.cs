using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.CreateUserRequestTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task SuccessfullyCreateAUser()
    {
        // Arrange
        var handler = new CreateUserRequestHandler(
            new UserService(
                new NullLogger<UserService>(),
                new FakeTimeProvider(),
                new InMemoryUserRepository(),
                new InMemoryUserRelationshipRepository()
            )
        );
        var request = new CreateUserRequest(
            new("1e2d19e5-d92d-42da-a927-4686d3542453"),
            "A. N. Other",
            new PhoneNumber("+1234567890")
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
    }
}
