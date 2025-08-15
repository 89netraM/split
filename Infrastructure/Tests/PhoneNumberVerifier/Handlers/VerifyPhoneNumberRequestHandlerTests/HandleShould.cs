using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Infrastructure.PhoneNumberVerifier.Handlers;
using Split.Infrastructure.PhoneNumberVerifier.Services;
using Split.Infrastructure.Tests.PhoneNumberVerifier.Builders;

namespace Split.Infrastructure.Tests.PhoneNumberVerifier.Handlers.VerifyPhoneNumberRequestHandlerTests;

[TestClass]
public sealed class HandleShould
{
    [TestMethod]
    public async Task VerifyAValidRequest()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 08, 15, 07, 23, 00, new(00, 00, 00)));
        var random = new Random(0);
        var options = Options.Create(new PhoneNumberVerificationOptions() { CodeLength = 6, CodeCharacters = "A" });
        var encryptionService = EncryptionServiceBuilder.Build();
        var codeSenderService = CodeSenderServiceBuilder.Build();
        var phoneNumberVerificationService = new PhoneNumberVerificationService(
            new NullLogger<PhoneNumberVerificationService>(),
            timeProvider,
            random,
            options,
            encryptionService,
            codeSenderService
        );
        var phoneNumber = new PhoneNumber("+46123456789");
        var context = await phoneNumberVerificationService.CreateVerification(phoneNumber, CancellationToken.None);
        var code = new PhoneNumberVerificationCode("AAAAAA");

        var handler = new VerifyPhoneNumberRequestHandler(phoneNumberVerificationService);

        // Act
        var result = await handler.Handle(new(code.Code, context.Context), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.PhoneNumber);
        Assert.AreEqual(phoneNumber, result.PhoneNumber);
    }
}
