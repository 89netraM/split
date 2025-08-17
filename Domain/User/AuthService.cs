using System;
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
        var context = CreateContext(challenge, verificationResponse.PhoneNumber);
        return new(challenge, context, user is not null);
    }

    private string CreateContext(CredentialCreateOptions challenge, PhoneNumber phoneNumber) =>
        encryptionService.Encrypt(
            JsonSerializer.Serialize(
                new ChallengeContext(challenge, phoneNumber),
                ChallengeContextJsonSerializerContext.Default.ChallengeContext
            )
        );

    internal async Task CreateUserAndAuthKey(
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

    internal async Task CreateAuthKey(
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

    private async Task CreateAuthKey(
        AuthenticatorAttestationRawResponse attestation,
        string challengeContext,
        Func<UserAggregate?, ChallengeContext, CancellationToken, Task<UserAggregate>> ensureUser,
        CancellationToken cancellationToken
    )
    {
        var context = DecodeContext(challengeContext);
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
    }

    private ChallengeContext DecodeContext(string challengeContext) =>
        JsonSerializer.Deserialize(
            encryptionService.Decrypt(challengeContext),
            ChallengeContextJsonSerializerContext.Default.ChallengeContext
        ) ?? throw new CreateUserAuthKeyContextNullException();
}

internal sealed record ChallengeCreationResult(
    CredentialCreateOptions Challenge,
    string ChallengeContext,
    bool UserExists
);

internal sealed record UserCreationInformation(string Name);

internal sealed record ChallengeContext(CredentialCreateOptions Challenge, PhoneNumber PhoneNumber);

[JsonSerializable(typeof(ChallengeContext))]
internal sealed partial class ChallengeContextJsonSerializerContext : JsonSerializerContext;

internal sealed class CreateUserAuthKeyContextNullException()
    : Exception("Deserializing decrypted context returned null.");

internal sealed class MissingUserException(PhoneNumber phoneNumber)
    : Exception($"User missing for phone number {phoneNumber.Value} when adding auth key.");

internal sealed class CredentialCreationFailedException(string errorMessage, PhoneNumber phoneNumber)
    : Exception($"Credential creation failed with message \"{errorMessage}\" for phone number {phoneNumber.Value}.");
