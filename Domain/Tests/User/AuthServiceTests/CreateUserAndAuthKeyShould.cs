using System.Linq;
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
public sealed class CreateUserAndAuthKeyShould
{
    [TestMethod]
    public async Task CreateAUserWithAnAuthKey()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var userRepository = new InMemoryUserRepository([]);
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 13, 57, 00, new(00, 00, 00))),
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
            .Returns("""{ "Challenge": { "user": { "id": "TQ==" } }, "PhoneNumber": { "Value": "+46123456789" } }""");
        var authService = new AuthService(sender, userRepository, userService, fido2, encryptionService);

        // Act
        await authService.CreateUserAndAuthKey(new(), "secret", new("A. N. Other"), CancellationToken.None);

        // Assert
        Assert.ContainsSingle(userRepository.Users);
        var user = userRepository.Users.Values.First();
        Assert.ContainsSingle(user.AuthKeys);
    }

    [TestMethod]
    public async Task ThrowWhenFido2CreationFails()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        var userRepository = new InMemoryUserRepository([]);
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 13, 57, 00, new(00, 00, 00))),
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
            .Returns("""{ "Challenge": { "user": { "id": "TQ==" } }, "PhoneNumber": { "Value": "+46123456789" } }""");
        var authService = new AuthService(sender, userRepository, userService, fido2, encryptionService);

        // Act
        async Task act() =>
            await authService.CreateUserAndAuthKey(new(), "secret", new("A. N. Other"), CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<CredentialCreationFailedException>(act);
    }
}
