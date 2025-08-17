using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Mediator;
using Split.Domain.Primitives;
using Split.Domain.User.Events;

namespace Split.Domain.User;

public sealed class AuthService(
    ISender sender,
    IUserRepository userRepository,
    UserService userService,
    IFido2 fido2,
    IEncryptionService encryptionService
)
{
    internal async Task<ChallengeCreationResult?> CreateChallenge(
        string phoneNumberVerificationCode,
        string phoneNumberVerificationContext,
        CancellationToken cancellationToken
    )
    {
        var verificationResponse = await sender.Send(
            new VerifyPhoneNumberRequest(phoneNumberVerificationCode, phoneNumberVerificationContext),
            cancellationToken
        );
        if (!verificationResponse.Success)
        {
            return null;
        }
        var user = await userRepository.GetUserByPhoneNumberAsync(verificationResponse.PhoneNumber, cancellationToken);
        var challenge = fido2.RequestNewCredential(
            user: new()
            {
                DisplayName = verificationResponse.PhoneNumber.Value,
                Id = Encoding.UTF8.GetBytes(user is { Id.Value: var id } ? id : Guid.CreateVersion7().ToString()),
            },
            excludeCredentials: [],
            authenticatorSelection: new() { UserVerification = UserVerificationRequirement.Required },
            attestationPreference: AttestationConveyancePreference.Direct
        );
        var context = CreateChallengeContext(challenge, verificationResponse.PhoneNumber);
        return new(challenge, context, user is not null);
    }

    private string CreateChallengeContext(CredentialCreateOptions challenge, PhoneNumber phoneNumber) =>
        encryptionService.Encrypt(
            JsonSerializer.Serialize(
                new ChallengeContext(challenge, phoneNumber),
                ContextJsonSerializerContext.Default.ChallengeContext
            )
        );

    internal async Task<UserAggregate> CreateUserAndAuthKey(
        AuthenticatorAttestationRawResponse attestation,
        string challengeContext,
        UserCreationInformation userInfo,
        CancellationToken cancellationToken
    ) =>
        await CreateAuthKey(
            attestation,
            challengeContext,
            async (u, c, ct) =>
                u
                ?? await userService.CreateUserAsync(
                    new(Guid.CreateVersion7().ToString()),
                    userInfo.Name,
                    c.PhoneNumber,
                    ct
                ),
            cancellationToken
        );

    internal async Task<UserAggregate> CreateAuthKey(
        AuthenticatorAttestationRawResponse attestation,
        string challengeContext,
        CancellationToken cancellationToken
    ) =>
        await CreateAuthKey(
            attestation,
            challengeContext,
            (u, c, _) =>
                u is not null
                    ? Task.FromResult(u)
                    : Task.FromException<UserAggregate>(new MissingUserException(c.PhoneNumber)),
            cancellationToken
        );

    private async Task<UserAggregate> CreateAuthKey(
        AuthenticatorAttestationRawResponse attestation,
        string challengeContext,
        Func<UserAggregate?, ChallengeContext, CancellationToken, Task<UserAggregate>> ensureUser,
        CancellationToken cancellationToken
    )
    {
        var context = DecodeChallengeContext(challengeContext);
        var userId = new UserId(Encoding.UTF8.GetString(context.Challenge.User.Id));
        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        var credentialResult = await fido2.MakeNewCredentialAsync(
            attestation,
            context.Challenge,
            isCredentialIdUniqueToUser: async (p, ct) =>
                !await userRepository.DoesAuthKeyIdExist(new(Convert.ToBase64String(p.CredentialId)), ct),
            cancellationToken: cancellationToken
        );
        if (credentialResult.Result is not { } credential)
        {
            throw new CredentialCreationFailedException(credentialResult.ErrorMessage, context.PhoneNumber);
        }

        user = await ensureUser(user, context, cancellationToken);

        user.AddAuthKey(new(Convert.ToBase64String(credential.CredentialId)), credential.PublicKey, credential.Counter);
        await userRepository.SaveAsync(user, cancellationToken);

        return user;
    }

    private ChallengeContext DecodeChallengeContext(string challengeContext) =>
        JsonSerializer.Deserialize(
            encryptionService.Decrypt(challengeContext),
            ContextJsonSerializerContext.Default.ChallengeContext
        ) ?? throw new CreateUserAuthKeyContextNullException();

    internal async Task<AssertionChallenge> CreateAssertion(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken
    )
    {
        var user =
            await userService.GetUserByPhoneNumberAsync(phoneNumber, cancellationToken)
            ?? throw new MissingUserException(phoneNumber);

        var assertion = fido2.GetAssertionOptions(
            user.AuthKeys.Select(k => new PublicKeyCredentialDescriptor(Convert.FromBase64String(k.Id.Value))),
            UserVerificationRequirement.Discouraged
        );

        return new(assertion, CreateAssertionContext(assertion, user.Id));
    }

    private string CreateAssertionContext(AssertionOptions assertion, UserId userId) =>
        encryptionService.Encrypt(
            JsonSerializer.Serialize(
                new AssertionContext(assertion, userId),
                ContextJsonSerializerContext.Default.AssertionContext
            )
        );

    internal async Task<UserAggregate> AssertAssertion(
        AuthenticatorAssertionRawResponse assertionResponse,
        string assertionContext,
        CancellationToken cancellationToken
    )
    {
        var context = DecodeAssertionContext(assertionContext);
        var user =
            await userService.GetUserAsync(context.UserId, cancellationToken)
            ?? throw new MissingUserException(context.UserId);
        var authKeyId = new AuthKeyId(Convert.ToBase64String(assertionResponse.RawId));
        var authKey =
            user.AuthKeys.FirstOrDefault(k => k.Id == authKeyId) ?? throw new MissingKeyException(authKeyId, user.Id);
        var assertion = await fido2.MakeAssertionAsync(
            assertionResponse,
            context.Assertion,
            authKey.Key,
            authKey.SignCount,
            isUserHandleOwnerOfCredentialIdCallback: (param, _) =>
                Task.FromResult(new UserId(Encoding.UTF8.GetString(param.UserHandle)) == user.Id),
            cancellationToken: cancellationToken
        );
        authKey.IncreaseSignCount(assertion.Counter);
        await userRepository.SaveAsync(user, cancellationToken);

        return user;
    }

    private AssertionContext DecodeAssertionContext(string context) =>
        JsonSerializer.Deserialize(
            encryptionService.Decrypt(context),
            ContextJsonSerializerContext.Default.AssertionContext
        ) ?? throw new AssertUserAuthKeyContextNullException();
}

internal sealed record ChallengeCreationResult(
    CredentialCreateOptions Challenge,
    string ChallengeContext,
    bool UserExists
);

internal sealed record UserCreationInformation(string Name);

internal sealed record ChallengeContext(CredentialCreateOptions Challenge, PhoneNumber PhoneNumber);

internal sealed record AssertionChallenge(AssertionOptions Assertion, string AssertionContext);

internal sealed record AssertionContext(AssertionOptions Assertion, UserId UserId);

[JsonSerializable(typeof(ChallengeContext))]
[JsonSerializable(typeof(AssertionContext))]
internal sealed partial class ContextJsonSerializerContext : JsonSerializerContext;

internal sealed class CreateUserAuthKeyContextNullException()
    : Exception("Deserializing decrypted context returned null.");

internal sealed class AssertUserAuthKeyContextNullException()
    : Exception("Deserializing decrypted context returned null.");

internal sealed class MissingUserException : Exception
{
    public MissingUserException(PhoneNumber phoneNumber)
        : base($"User missing for phone number {phoneNumber.Value}.") { }

    public MissingUserException(UserId userId)
        : base($"User missing for id {userId.Value}.") { }
}

internal sealed class CredentialCreationFailedException(string errorMessage, PhoneNumber phoneNumber)
    : Exception($"Credential creation failed with message \"{errorMessage}\" for phone number {phoneNumber.Value}.");

internal sealed class MissingKeyException(AuthKeyId authKeyId, UserId userId)
    : Exception($"No key with id {authKeyId.Value} exists on user {userId.Value}.");
