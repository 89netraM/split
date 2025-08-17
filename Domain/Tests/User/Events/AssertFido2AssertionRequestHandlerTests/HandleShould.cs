using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.AssertFido2AssertionRequestHandlerTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task AssertAValidAssertion()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var user = new UserAggregate(
            new("User-Id"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 17, 10, 58, 00, new(00, 00, 00))
        );
        var authKeyId = new AuthKeyId("TQ==");
        user.AddAuthKey(authKeyId, [0x00], 0);
        var userRepository = new InMemoryUserRepository([user]);
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 17, 10, 58, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var fido2 = Substitute.For<IFido2>();
        fido2
            .MakeAssertionAsync(
                Arg.Any<AuthenticatorAssertionRawResponse>(),
                Arg.Any<AssertionOptions>(),
                Arg.Any<byte[]>(),
                Arg.Any<uint>(),
                isUserHandleOwnerOfCredentialIdCallback: Arg.Any<IsUserHandleOwnerOfCredentialIdAsync>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(new AssertionVerificationResult() { Counter = 1 });
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService
            .Decrypt(Arg.Any<string>())
            .Returns(
                """
                {
                    "Assertion": null,
                    "UserId": {
                        "Value": "User-Id"
                    }
                }
                """
            );
        var assertionResponse = new AuthenticatorAssertionRawResponse() { RawId = [(byte)'M'] };
        var authService = new AuthService(sender, userRepository, userService, fido2, encryptionService);
        var handler = new AssertFido2AssertionRequestHandler(authService);

        // Act
        var response = await handler.Handle(new(assertionResponse, "secret"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(1u, user.AuthKeys.Single().SignCount);
    }
}
