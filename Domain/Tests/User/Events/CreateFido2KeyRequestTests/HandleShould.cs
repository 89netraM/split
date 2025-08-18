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
using Split.Domain.User.Events;
using static Fido2NetLib.Fido2;

namespace Split.Domain.Tests.User.Events.CreateFido2KeyRequestTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task CreateAnAuthKey()
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
        var handler = new CreateFido2KeyRequestHandler(authService);

        // Act
        var response = await handler.Handle(new(new(), "secret"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.ContainsSingle(userRepository.Users);
        Assert.ContainsSingle(user.AuthKeys);
    }
}
