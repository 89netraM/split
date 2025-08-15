using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Mediator;
using Split.Domain.User.Events;

namespace Split.Domain.User;

public sealed class AuthService(ISender sender, IFido2 fido2, IEncryptionService encryptionService)
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
        var user = (await sender.Send(new PhoneNumberQuery(verificationResponse.PhoneNumber), cancellationToken)).User;
        var challenge = fido2.RequestNewCredential(
            user: new()
            {
                DisplayName = verificationResponse.PhoneNumber.Value,
                Id = user is { Id.Value: var id } ? Encoding.UTF8.GetBytes(id) : Guid.CreateVersion7().ToByteArray(),
            },
            excludeCredentials: [],
            authenticatorSelection: new() { UserVerification = UserVerificationRequirement.Required },
            attestationPreference: AttestationConveyancePreference.Direct
        );
        var context = CreateContext(challenge);
        return new(challenge, context, user is not null);
    }

    private string CreateContext(CredentialCreateOptions challenge) =>
        encryptionService.Encrypt(
            JsonSerializer.Serialize(
                new ChallengeContext(challenge),
                ChallengeContextJsonSerializerContext.Default.ChallengeContext
            )
        );
}

internal sealed record ChallengeCreationResult(
    CredentialCreateOptions Challenge,
    string ChallengeContext,
    bool UserExists
);

internal sealed record ChallengeContext(CredentialCreateOptions Challenge);

[JsonSerializable(typeof(ChallengeContext))]
internal sealed partial class ChallengeContextJsonSerializerContext : JsonSerializerContext;
