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
using static Fido2NetLib.Fido2;

namespace Split.Domain.Tests.User.Events.CreateFido2KeyAndUserRequestTests;

[TestClass]
public class HandleShould
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
        var handler = new CreateFido2KeyAndUserRequestHandler(authService);

        // Act
        var response = await handler.Handle(new(new(), "secret", "A. N. Other"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.ContainsSingle(userRepository.Users);
        var user = userRepository.Users.Values.First();
        Assert.ContainsSingle(user.AuthKeys);
    }
}
