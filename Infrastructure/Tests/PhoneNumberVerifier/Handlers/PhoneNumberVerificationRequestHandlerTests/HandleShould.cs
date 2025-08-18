using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Infrastructure.PhoneNumberVerifier.Handlers;
using Split.Infrastructure.PhoneNumberVerifier.Services;
using Split.Infrastructure.Tests.PhoneNumberVerifier.Builders;

namespace Split.Infrastructure.Tests.PhoneNumberVerifier.Handlers.PhoneNumberVerificationRequestHandlerTests;

[TestClass]
public sealed class HandleShould
{
    [TestMethod]
    public async Task ReturnAVerificationChallenge()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider(new(2025, 08, 14, 14, 43, 00, new(00, 00, 00)));
        var random = new Random(0);
        var options = Options.Create(new PhoneNumberVerificationOptions());
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

        var handler = new PhoneNumberVerificationRequestHandler(phoneNumberVerificationService);

        // Act
        var result = await handler.Handle(new(new("+46123456789")), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.VerificationContext);
        Assert.IsNotEmpty(result.VerificationContext);
    }
}
