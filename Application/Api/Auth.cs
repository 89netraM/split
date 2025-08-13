using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Split.Application.Api;

public static class Auth
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/auth");

        group.MapGet("/credential", GetCreationChallenge);
        group.MapPost("/credential", CreateCredential);

        group.MapGet("/assertion", GetAssertionChallenge);
        group.MapPost("/assertion", Assert);

        return group;
    }

    private const string Fido2CredentialSessionKey = nameof(Fido2CredentialSessionKey);
    private const string Fido2AssertionSessionKey = nameof(Fido2AssertionSessionKey);

    private static IResult GetCreationChallenge(
        [FromServices] IFido2 fido2,
        [FromServices] EncryptionService encryptionService,
        [FromQuery] string phoneNumber
    )
    {
        var options = fido2.RequestNewCredential(
            user: new()
            {
                DisplayName = phoneNumber,
                Id = Encoding.UTF8.GetBytes(phoneNumber),
                Name = phoneNumber,
            },
            excludeCredentials: [],
            authenticatorSelection: new() { UserVerification = UserVerificationRequirement.Required },
            attestationPreference: AttestationConveyancePreference.Direct
        );

        return Results.Json(new GetCreationChallengeResponse(encryptionService.Encrypt(options.ToJson()), options));
    }

    private static async Task<IResult> CreateCredential(
        [FromServices] IFido2 fido2,
        [FromServices] EncryptionService encryptionService,
        [FromBody] CreateCredentialRequest request,
        CancellationToken cancellationToken
    )
    {
        var createOptions = CredentialCreateOptions.FromJson(encryptionService.Decrypt(request.Context));

        var credentialResult = await fido2.MakeNewCredentialAsync(
            request.Attestation,
            createOptions,
            isCredentialIdUniqueToUser: (_, _) => Task.FromResult(true),
            cancellationToken: cancellationToken
        );
        if (credentialResult.Result is not { } credential)
        {
            return Results.BadRequest();
        }
        var user = Users[credential.User.Name] = Users.GetValueOrDefault(credential.User.Name, []);
        user[Convert.ToBase64String(credential.CredentialId)] = (credential.PublicKey, credential.Counter);
        return Results.Ok();
    }

    private static IResult GetAssertionChallenge(
        [FromServices] IFido2 fido2,
        [FromServices] EncryptionService encryptionService,
        [FromQuery] string phoneNumber
    )
    {
        var options = fido2.GetAssertionOptions(
            Users[phoneNumber].Keys.Select(p => new PublicKeyCredentialDescriptor(Convert.FromBase64String(p))),
            UserVerificationRequirement.Discouraged
        );
        return Results.Json(new GetAssertionChallengeResponse(encryptionService.Encrypt(options.ToJson()), options));
    }

    private static async Task<IResult> Assert(
        [FromServices] IFido2 fido2,
        [FromServices] EncryptionService encryptionService,
        [FromServices] JwtService jwtService,
        [FromBody] AssertRequest request,
        CancellationToken cancellationToken
    )
    {
        var assertionOptions = AssertionOptions.FromJson(encryptionService.Decrypt(request.Context));
        var user = Users.Values.FirstOrDefault(u => u.ContainsKey(Convert.ToBase64String(request.Assertion.RawId)));
        if (user is null)
        {
            return Results.NotFound();
        }
        var credential = user[Convert.ToBase64String(request.Assertion.RawId)];
        var assertion = await fido2.MakeAssertionAsync(
            request.Assertion,
            assertionOptions,
            credential.Item1,
            credential.Item2,
            isUserHandleOwnerOfCredentialIdCallback: (_, cts) => Task.FromResult(true),
            cancellationToken: cancellationToken
        );
        user[Convert.ToBase64String(request.Assertion.RawId)] = (credential.Item1, assertion.Counter);
        return Results.Json(
            new AssertResponse(
                jwtService.GenerateToken(
                    new(
                        new(Convert.ToBase64String(request.Assertion.RawId)),
                        "Mårten Åsberg",
                        new("+46722177038"),
                        DateTimeOffset.UtcNow
                    )
                )
            )
        );
    }

    private static readonly Dictionary<string, Dictionary<string, (byte[], uint)>> Users = [];
}

[JsonSerializable(typeof(GetCreationChallengeResponse))]
[JsonSerializable(typeof(CreateCredentialRequest))]
[JsonSerializable(typeof(GetAssertionChallengeResponse))]
[JsonSerializable(typeof(AssertRequest))]
[JsonSerializable(typeof(AssertResponse))]
public sealed partial class AuthSerializerContext : JsonSerializerContext;

public sealed record GetCreationChallengeResponse(
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("options")] CredentialCreateOptions Options
);

public sealed record CreateCredentialRequest(
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("attestation")] AuthenticatorAttestationRawResponse Attestation
);

public sealed record GetAssertionChallengeResponse(
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("options")] AssertionOptions Options
);

public sealed record AssertRequest(
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("assertion")] AuthenticatorAssertionRawResponse Assertion
);

public sealed record AssertResponse([property: JsonPropertyName("token")] string Token);
