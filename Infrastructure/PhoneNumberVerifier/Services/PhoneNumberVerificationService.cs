using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Infrastructure.PhoneNumberVerifier.Services;

public sealed class PhoneNumberVerificationService(
    ILogger<PhoneNumberVerificationService> logger,
    TimeProvider timeProvider,
    Random random,
    IOptions<PhoneNumberVerificationOptions> options,
    IEncryptionService encryptionService,
    CodeSenderService codeSenderService
)
{
    private readonly int codeLength = options.Value.CodeLength;
    private readonly string codeCharacters = options.Value.CodeCharacters;
    private readonly TimeSpan codeTimeout = options.Value.CodeTimeout;

    internal async Task<PhoneNumberVerificationContext> CreateVerification(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken
    )
    {
        var code = GenerateCode();
        var context = CreateContext(phoneNumber, code);
        await codeSenderService.SendCode(phoneNumber, code, cancellationToken);
        return context;
    }

    private PhoneNumberVerificationCode GenerateCode() =>
        new(
            string.Create(
                codeLength,
                (codeCharacters, random),
                (str, state) => state.random.GetItems(state.codeCharacters, str)
            )
        );

    private PhoneNumberVerificationContext CreateContext(PhoneNumber phoneNumber, PhoneNumberVerificationCode code)
    {
        var context = new Context(timeProvider.GetUtcNow(), code, phoneNumber);
        var contextJson = JsonSerializer.Serialize(context, ContextJsonSerializerContext.Default.Context);
        var contextString = encryptionService.Encrypt(contextJson);
        return new(contextString);
    }

    internal PhoneNumberVerificationResult VerifyPhoneNumber(
        PhoneNumberVerificationCode code,
        PhoneNumberVerificationContext context
    )
    {
        var decodedContext = DecodeContext(context);

        var now = timeProvider.GetUtcNow();
        if (decodedContext.CreatedAt + codeTimeout < now)
        {
            logger.LogDebug(
                "Code created too long ago, {CreatedAt} + {CodeTimeout} < {Now}",
                decodedContext.CreatedAt,
                codeTimeout,
                now
            );
            return new(null);
        }

        if (decodedContext.Code != code)
        {
            logger.LogDebug(
                "Wrong code {WrongCode} for phone number {PhoneNumber} verification",
                code.Code,
                decodedContext.PhoneNumber
            );
            return new(null);
        }

        return new(decodedContext.PhoneNumber);
    }

    private Context DecodeContext(PhoneNumberVerificationContext context) =>
        JsonSerializer.Deserialize(
            encryptionService.Decrypt(context.Context),
            ContextJsonSerializerContext.Default.Context
        ) ?? throw new PhoneNumberVerificationContextNullException();
}

internal record Context(DateTimeOffset CreatedAt, PhoneNumberVerificationCode Code, PhoneNumber PhoneNumber);

[JsonSerializable(typeof(Context))]
internal partial class ContextJsonSerializerContext : JsonSerializerContext;

public sealed class PhoneNumberVerificationOptions
{
    [Range(1, int.MaxValue)]
    public int CodeLength { get; set; } = 6;

    [StringLength(int.MaxValue, MinimumLength = 1)]
    public string CodeCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public TimeSpan CodeTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

[OptionsValidator]
internal partial class PhoneNumberVerificationOptionsValidator : IValidateOptions<PhoneNumberVerificationOptions>;

file sealed class PhoneNumberVerificationContextNullException()
    : Exception("Deserializing decrypted context returned null.");
