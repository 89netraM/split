using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.User;
using Split.Domain.User.Events;

namespace Split.Domain.Tests.User.Events.CreateFido2ChallengeRequestTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task CreateAChallenge()
    {
        // Arrange
        var sender = Substitute.For<ISender>();
        sender
            .Send(Arg.Any<VerifyPhoneNumberRequest>(), Arg.Any<CancellationToken>())
            .Returns(new VerifyPhoneNumberResponse(new("+46123456789")));
        sender
            .Send(Arg.Any<PhoneNumberQuery>(), Arg.Any<CancellationToken>())
            .Returns(new PhoneNumberQueryResult(null));
        var fido2 = Substitute.For<IFido2>();
        var encryptionService = Substitute.For<IEncryptionService>();
        encryptionService.Encrypt(Arg.Any<string>()).Returns("secret");
        var authService = new AuthService(sender, fido2, encryptionService);
        var handler = new CreateFido2ChallengeRequestHandler(authService);

        // Act
        var response = await handler.Handle(new("aCode1", "phoneContext"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(response.Result);
    }
}
