using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using static Fido2NetLib.Fido2;

namespace Split.Domain.Tests.User.AuthServiceTests;

[TestClass]
public sealed class CreateAuthKeyShould
{
    [TestMethod]
    public async Task AddAnAuthKeyToTheUser()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var user = new UserAggregate(
            new("User-Id"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 15, 14, 20, 00, new(00, 00, 00))
        );
        var userRepository = new InMemoryUserRepository([user]);
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 14, 19, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var fido2 = Substitute.For<IFido2>();
        fido2
            .MakeNewCredentialAsync(
                Arg.Any<AuthenticatorAttestationRawResponse>(),
                Arg.Any<CredentialCreateOptions>(),
                isCredentialIdUniqueToUser: Arg.Any<IsCredentialIdUniqueToUserAsyncDelegate>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(
                new CredentialMakeResult(
                    "Success",
                    "",
                    new()
                    {
                        CredentialId = [0x00],
                        PublicKey = [0x00],
                        Counter = 0,
                    }
                )
            );
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService
            .Decrypt(Arg.Any<string>())
            .Returns(
                $$"""
                {
                    "Challenge": {
                        "user": {
                            "id": "{{Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id.Value))}}"
                        }
                    },
                    "PhoneNumber": {
                        "Value": "{{user.PhoneNumber.Value}}"
                    }
                }
                """
            );
        var authService = new AuthService(sender, userRepository, userService, fido2, encryptionService);

        // Act
        await authService.CreateAuthKey(new(), "secret", CancellationToken.None);

        // Assert
        Assert.ContainsSingle(userRepository.Users);
        Assert.ContainsSingle(user.AuthKeys);
    }

    [TestMethod]
    public async Task ThrowWhenTheUserIdDoesNotExist()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var user = new UserAggregate(
            new("User-Id"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 15, 14, 20, 00, new(00, 00, 00))
        );
        var userRepository = new InMemoryUserRepository([user]);
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 14, 19, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var fido2 = Substitute.For<IFido2>();
        fido2
            .MakeNewCredentialAsync(
                Arg.Any<AuthenticatorAttestationRawResponse>(),
                Arg.Any<CredentialCreateOptions>(),
                isCredentialIdUniqueToUser: Arg.Any<IsCredentialIdUniqueToUserAsyncDelegate>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(
                new CredentialMakeResult(
                    "Success",
                    "",
                    new()
                    {
                        CredentialId = [0x00],
                        PublicKey = [0x00],
                        Counter = 0,
                    }
                )
            );
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService
            .Decrypt(Arg.Any<string>())
            .Returns(
                $$"""{ "Challenge": { "user": { "id": "TQ==" } }, "PhoneNumber": { "Value": "{{user.PhoneNumber.Value}}" } }"""
            );
        var authService = new AuthService(sender, userRepository, userService, fido2, encryptionService);

        // Act
        async Task act() => await authService.CreateAuthKey(new(), "secret", CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<MissingUserException>(act);
    }

    [TestMethod]
    public async Task ThrowWhenFido2CreationFails()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var user = new UserAggregate(
            new("User-Id"),
            "A. N. Other",
            new("+46123456789"),
            new(2025, 08, 15, 14, 20, 00, new(00, 00, 00))
        );
        var userRepository = new InMemoryUserRepository([user]);
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 14, 19, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var fido2 = Substitute.For<IFido2>();
        fido2
            .MakeNewCredentialAsync(
                Arg.Any<AuthenticatorAttestationRawResponse>(),
                Arg.Any<CredentialCreateOptions>(),
                isCredentialIdUniqueToUser: Arg.Any<IsCredentialIdUniqueToUserAsyncDelegate>(),
                cancellationToken: Arg.Any<CancellationToken>()
            )
            .Returns(new CredentialMakeResult("Failure", "Failed for some fake reason.", null));
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService
            .Decrypt(Arg.Any<string>())
            .Returns(
                $$"""
                {
                    "Challenge": {
                        "user": {
                            "id": "{{Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Id.Value))}}"
                        }
                    },
                    "PhoneNumber": {
                        "Value": "{{user.PhoneNumber.Value}}"
                    }
                }
                """
            );
        var authService = new AuthService(sender, userRepository, userService, fido2, encryptionService);

        // Act
        async Task act() => await authService.CreateAuthKey(new(), "secret", CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<CredentialCreationFailedException>(act);
    }
}
