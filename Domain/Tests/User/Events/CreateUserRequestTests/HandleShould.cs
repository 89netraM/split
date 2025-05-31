using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
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
                Substitute.For<ILogger<UserService>>(),
                new FakeTimeProvider(),
                Substitute.For<IUserRepository>()
            )
        );
        var request = new CreateUserRequest(
            new("1e2d19e5-d92d-42da-a927-4686d3542453"),
            "A. N. Other",
            new PhoneNumber("1234567890")
        );

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
    }
}
