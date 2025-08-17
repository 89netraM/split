using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.CreateFido2AssertionRequestHandlerTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task CreateAnAssertionChallengeAndContext()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var user = new UserAggregate(
            new("User-Id"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 17, 10, 58, 00, new(00, 00, 00))
        );
        var userRepository = new InMemoryUserRepository([user]);
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 17, 10, 58, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var fido2 = Substitute.For<IFido2>();
        fido2
            .GetAssertionOptions(
                Arg.Any<IEnumerable<PublicKeyCredentialDescriptor>>(),
                Arg.Any<UserVerificationRequirement>()
            )
            .Returns(new AssertionOptions());
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Encrypt(Arg.Any<string>()).Returns("secret");
        var authService = new AuthService(sender, userRepository, userService, fido2, encryptionService);
        var handler = new CreateFido2AssertionRequestHandler(authService);

        // Act
        var response = await handler.Handle(new(user.PhoneNumber), CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Assertion);
        Assert.IsNotEmpty(response.AssertionContext);
    }
}
