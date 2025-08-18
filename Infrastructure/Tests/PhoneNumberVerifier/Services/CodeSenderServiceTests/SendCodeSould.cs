using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using HuaweiWifiSms.Grpc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Split.Domain.Primitives;
using Split.Infrastructure.PhoneNumberVerifier.Services;

namespace Split.Infrastructure.Tests.PhoneNumberVerifier.Services.CodeSenderServiceTests;

[TestClass]
public sealed class SendCodeShould
{
    [TestMethod]
    public async Task SendTheCodeToTheSmsSenderClient()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46123456789");
        var code = new PhoneNumberVerificationCode("123456");
        var logger = new NullLogger<CodeSenderService>();
        var environment = new HostingEnvironment() { EnvironmentName = "Production" };
        var options = Options.Create(new CodeSenderOptions { MessageFormat = "{0}" });
        var smsSenderClient = Substitute.For<SmsSender.SmsSenderClient>();
        smsSenderClient
            .SendSmsAsync(
                Arg.Is<SmsRequest>(r => r.RecipientPhoneNumber == phoneNumber.Value && r.Content == code.Code),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new AsyncUnaryCall<SmsResponse>(
                    Task.FromResult(new SmsResponse() { Status = SmsStatus.Success }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => [],
                    () => { }
                )
            );
        var codeSenderService = new CodeSenderService(logger, environment, options, smsSenderClient);

        // Act
        await codeSenderService.SendCode(phoneNumber, code, CancellationToken.None);

        // Assert
        _ = smsSenderClient
            .Received(1)
            .SendSmsAsync(
                Arg.Is<SmsRequest>(r => r.RecipientPhoneNumber == phoneNumber.Value && r.Content == code.Code),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>()
            );
    }

    [TestMethod]
    public async Task ThrowWhenNoStatusIsReturned()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46123456789");
        var code = new PhoneNumberVerificationCode("123456");
        var logger = new NullLogger<CodeSenderService>();
        var environment = new HostingEnvironment() { EnvironmentName = "Production" };
        var options = Options.Create(new CodeSenderOptions { MessageFormat = "{0}" });
        var smsSenderClient = Substitute.For<SmsSender.SmsSenderClient>();
        smsSenderClient
            .SendSmsAsync(
                Arg.Is<SmsRequest>(r => r.RecipientPhoneNumber == phoneNumber.Value && r.Content == code.Code),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new AsyncUnaryCall<SmsResponse>(
                    Task.FromResult(new SmsResponse()),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => [],
                    () => { }
                )
            );
        var codeSenderService = new CodeSenderService(logger, environment, options, smsSenderClient);

        // Act
        async Task act() => await codeSenderService.SendCode(phoneNumber, code, CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<SendSmsException>(act);
    }

    [TestMethod]
    [DataRow(SmsStatus.Unknown)]
    [DataRow(SmsStatus.Failure)]
    public async Task ThrowWhenANonSuccessStatusIsReturned(SmsStatus status)
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46123456789");
        var code = new PhoneNumberVerificationCode("123456");
        var logger = new NullLogger<CodeSenderService>();
        var environment = new HostingEnvironment() { EnvironmentName = "Production" };
        var options = Options.Create(new CodeSenderOptions { MessageFormat = "{0}" });
        var smsSenderClient = Substitute.For<SmsSender.SmsSenderClient>();
        smsSenderClient
            .SendSmsAsync(
                Arg.Is<SmsRequest>(r => r.RecipientPhoneNumber == phoneNumber.Value && r.Content == code.Code),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(
                new AsyncUnaryCall<SmsResponse>(
                    Task.FromResult(new SmsResponse() { Status = status }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => [],
                    () => { }
                )
            );
        var codeSenderService = new CodeSenderService(logger, environment, options, smsSenderClient);

        // Act
        async Task act() => await codeSenderService.SendCode(phoneNumber, code, CancellationToken.None);

        // Assert
        await Assert.ThrowsExactlyAsync<SendSmsException>(act);
    }

    [TestMethod]
    public async Task NotSendTheCodeToTheSmsSenderClient_WhenInDevelopmentEnvironment()
    {
        // Arrange
        var phoneNumber = new PhoneNumber("+46123456789");
        var code = new PhoneNumberVerificationCode("123456");
        var logger = new NullLogger<CodeSenderService>();
        var environment = new HostingEnvironment() { EnvironmentName = "Development" };
        var options = Options.Create(new CodeSenderOptions { MessageFormat = "{0}" });
        var smsSenderClient = Substitute.For<SmsSender.SmsSenderClient>();
        var codeSenderService = new CodeSenderService(logger, environment, options, smsSenderClient);

        // Act
        await codeSenderService.SendCode(phoneNumber, code, CancellationToken.None);

        // Assert
        _ = smsSenderClient
            .DidNotReceive()
            .SendSmsAsync(
                Arg.Any<SmsRequest>(),
                Arg.Any<Metadata>(),
                Arg.Any<DateTime?>(),
                Arg.Any<CancellationToken>()
            );
    }
}
