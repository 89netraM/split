using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using HuaweiWifiSms.Grpc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Split.Domain.Primitives;

namespace Split.Infrastructure.PhoneNumberVerifier.Services;

public sealed class CodeSenderService(
    ILogger<CodeSenderService> logger,
    IHostEnvironment environment,
    IOptions<CodeSenderOptions> options,
    SmsSender.SmsSenderClient smsSenderClient
)
{
    private readonly string messageFormat = options.Value.MessageFormat;

    public async Task SendCode(
        PhoneNumber phoneNumber,
        PhoneNumberVerificationCode code,
        CancellationToken cancellationToken
    )
    {
        if (environment.IsDevelopment())
        {
            logger.LogInformation(
                "Fake sending {Code} to {PhoneNumber} with format {MessageFormat}",
                code.Code,
                phoneNumber.Value,
                messageFormat
            );
            return;
        }

        var response = await smsSenderClient.SendSmsAsync(
            new() { RecipientPhoneNumber = phoneNumber.Value, Content = string.Format(messageFormat, code.Code) },
            cancellationToken: cancellationToken
        );
        if (!response.HasStatus || response.Status != SmsStatus.Success)
        {
            throw new SendSmsException(response.HasStatus ? response.Status : null);
        }
    }
}

public sealed class CodeSenderOptions
{
    [Required]
    public required string MessageFormat { get; set; }
}

[OptionsValidator]
internal sealed partial class CodeSenderOptionsValidator : IValidateOptions<CodeSenderOptions>;

internal sealed class SendSmsException(SmsStatus? smsStatus)
    : Exception(
        $"Failed to send sms, {(smsStatus is null ? "received no status code" : $"with status code {smsStatus}.")}"
    );
