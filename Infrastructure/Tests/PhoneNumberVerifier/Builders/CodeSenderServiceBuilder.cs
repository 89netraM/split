using HuaweiWifiSms.Grpc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Split.Infrastructure.PhoneNumberVerifier.Services;

namespace Split.Infrastructure.Tests.PhoneNumberVerifier.Builders;

internal static class CodeSenderServiceBuilder
{
    public static CodeSenderService Build(
        ILogger<CodeSenderService>? logger = null,
        IHostEnvironment? environment = null,
        IOptions<CodeSenderOptions>? options = null,
        SmsSender.SmsSenderClient? smsSenderClient = null
    ) =>
        new(
            logger ?? new NullLogger<CodeSenderService>(),
            environment ?? new HostingEnvironment() { EnvironmentName = "Development" },
            options ?? Options.Create(new CodeSenderOptions() { MessageFormat = "{0}" }),
            smsSenderClient ?? Substitute.For<SmsSender.SmsSenderClient>()
        );
}
