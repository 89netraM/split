using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Primitives;
using Split.Infrastructure.PhoneNumberVerifier.Services;
using Split.Infrastructure.Tests.PhoneNumberVerifier.Builders;

namespace Split.Infrastructure.Tests.PhoneNumberVerifier.Services.PhoneNumberVerificationServiceTests;

[TestClass]
public sealed class VerifyPhoneNumberShould
{
    [TestMethod]
    public async Task VerifyAValidContextAndCode()
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

        // Act
        var verificationResult = phoneNumberVerificationService.VerifyPhoneNumber(code, context);

        // Assert
        Assert.IsNotNull(verificationResult.PhoneNumber);
        Assert.AreEqual(phoneNumber, verificationResult.PhoneNumber);
    }

    [TestMethod]
    public async Task NotVerifyWithTheWrongCode()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 08, 15, 07, 37, 00, new(00, 00, 00)));
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
        var code = new PhoneNumberVerificationCode("BBBBBB");

        // Act
        var verificationResult = phoneNumberVerificationService.VerifyPhoneNumber(code, context);

        // Assert
        Assert.IsNull(verificationResult.PhoneNumber);
    }

    [TestMethod]
    public async Task NotVerifyWithACodeThatHasTimedOut()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 08, 15, 07, 37, 00, new(00, 00, 00)));
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
        timeProvider.Advance(options.Value.CodeTimeout * 2);
        var code = new PhoneNumberVerificationCode("AAAAAA");

        // Act
        var verificationResult = phoneNumberVerificationService.VerifyPhoneNumber(code, context);

        // Assert
        Assert.IsNull(verificationResult.PhoneNumber);
    }
}
