using System;
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
using NSubstitute.ExceptionExtensions;
using Split.Domain.Primitives;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;

namespace Split.Domain.Tests.User.AuthServiceTests;

[TestClass]
public sealed class AssertAssertionShould
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

        // Act
        await authService.AssertAssertion(assertionResponse, "secret", CancellationToken.None);

        // Assert
        Assert.AreEqual(1u, user.AuthKeys.Single().SignCount);
    }

    [TestMethod]
    public async Task ThrowWhenAssertingForADeletedUser()
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
        user.Remove(new(2025, 08, 17, 11, 18, 00, new(00, 00, 00)));
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

        // Act
        async Task act() => await authService.AssertAssertion(assertionResponse, "secret", CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<MissingUserException>(act);
    }

    [TestMethod]
    public async Task ThrowWhenAssertingForANonExistentKey()
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

        // Act
        async Task act() => await authService.AssertAssertion(assertionResponse, "secret", CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<MissingKeyException>(act);
    }

    [TestMethod]
    public async Task ThrowWhenAssertionFails()
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
            .Throws(new Fido2VerificationException());
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

        // Act
        async Task act() => await authService.AssertAssertion(assertionResponse, "secret", CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<Fido2VerificationException>(act);
    }
}
