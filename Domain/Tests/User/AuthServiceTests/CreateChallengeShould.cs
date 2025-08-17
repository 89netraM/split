using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
using Split.Domain.Tests.TestCommon;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.AuthServiceTests;

[TestClass]
public sealed class CreateChallengeShould
{
    [TestMethod]
    public async Task CreateAChallengeForANewUser()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender
            .Send(Arg.Any<VerifyPhoneNumberRequest>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyPhoneNumberResponse(new("+46123456789")));
        sender
            .Send(Arg.Any<PhoneNumberQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PhoneNumberQueryResult(null));
        var userRepository = Substitute.For<IUserRepository>();
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 12, 55, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Encrypt(Arg.Any<string>()).Returns("secret");
        var authService = new AuthService(
            sender,
            userRepository,
            userService,
            Substitute.For<IFido2>(),
            encryptionService
        );

        // Act
        var result = await authService.CreateChallenge("aCode1", "phoneContext", CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result.UserExists);
    }

    [TestMethod]
    public async Task CreateAChallengeForAnExistingUser()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46123456789");
        var sender = Substitute.For<ISender>();
        sender
            .Send(Arg.Any<VerifyPhoneNumberRequest>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyPhoneNumberResponse(phoneNumber));
        sender
            .Send(Arg.Any<PhoneNumberQuery>(), Arg.Any<CancellationToken>())
            .Returns(
                new PhoneNumberQueryResult(
                    new(new("User-ID"), "A. N. Other", phoneNumber, new(2025, 08, 15, 09, 41, 00, new(00, 00, 00)))
                )
            );
        var userRepository = new InMemoryUserRepository(
            [new(new("User-Id"), "A. N. Other", phoneNumber, new(2025, 08, 15, 12, 57, 00, new(00, 00, 00)))]
        );
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 12, 55, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Encrypt(Arg.Any<string>()).Returns("secret");
        var authService = new AuthService(
            sender,
            userRepository,
            userService,
            Substitute.For<IFido2>(),
            encryptionService
        );

        // Act
        var result = await authService.CreateChallenge("aCode1", "phoneContext", CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.UserExists);
    }

    [TestMethod]
    public async Task NotCreateAChallengeWhenFailingThePhoneNumberVerification()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender
            .Send(Arg.Any<VerifyPhoneNumberRequest>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyPhoneNumberResponse(null));
        var userRepository = Substitute.For<IUserRepository>();
        var userService = new UserService(
            new NullLogger<UserService>(),
            new FakeTimeProvider(new(2025, 08, 15, 12, 55, 00, new(00, 00, 00))),
            userRepository,
            Substitute.For<IUserRelationshipRepository>()
        );
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Encrypt(Arg.Any<string>()).Returns("secret");
        var authService = new AuthService(
            sender,
            userRepository,
            userService,
            Substitute.For<IFido2>(),
            encryptionService
        );

        // Act
        var result = await authService.CreateChallenge("aCode1", "phoneContext", CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }
}
