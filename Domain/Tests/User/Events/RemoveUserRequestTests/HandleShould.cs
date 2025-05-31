using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.RemoveUserRequestTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task SuccessfullyRemoveAUser()
    {
        // Arrange
        var handler = new RemoveUserRequestHandler(
            new UserService(
                Substitute.For<ILogger<UserService>>(),
                new FakeTimeProvider(),
                Substitute.For<IUserRepository>()
            )
        );
        var request = new RemoveUserRequest(new("6ef5ae5c-575a-4971-afeb-ee4fd0ccd2e4"));

        // Act & Assert
        await handler.Handle(request, CancellationToken.None);
    }
}
