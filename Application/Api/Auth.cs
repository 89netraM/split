using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Split.Domain.Primitives;
using Split.Domain.User.Events;

namespace Split.Application.Api;

public static class Auth
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/auth");

        group.MapGet("/verify-phone-number", RequestPhoneNumberVerification).Produces<VerifyPhoneNumberResponse>();
        group.MapPost("/credential", GetCredentialChallenge).Produces<GetCreationChallengeResponse>();
        group.MapPost("/credential/new", CreateNewUser).Produces<CreateNewUserResponse>();
        group.MapPost("/credential/existing", CreateExistingUser).Produces<CreateExistingUserResponse>();

        group.MapGet("/assertion", GetAssertionChallenge).Produces<GetAssertionChallengeResponse>();
        group.MapPost("/assertion", Assert).Produces<AssertResponse>();

        return group;
    }

    private static async Task<IResult> RequestPhoneNumberVerification(
        [FromServices] ISender sender,
        [FromQuery] string phoneNumber,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await sender.Send(new PhoneNumberVerificationRequest(new(phoneNumber)), cancellationToken);

            return Results.Json(new VerifyPhoneNumberResponse(response.VerificationContext));
        }
        catch (PhoneNumberFormatException)
        {
            return Results.BadRequest();
        }
    }

    private static async Task<IResult> GetCredentialChallenge(
        [FromServices] ISender sender,
        [FromBody] GetCreationChallengeRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await sender.Send(
            new CreateFido2ChallengeRequest(
                request.PhoneNumberVerificationCode,
                request.PhoneNumberVerificationContext
            ),
            cancellationToken
        );
        if (response.Result is null)
        {
            return Results.Unauthorized();
        }
        return Results.Json(
            new GetCreationChallengeResponse(
                response.Result.ChallengeContext,
                response.Result.Challenge,
                response.Result.UserExists
            )
        );
    }

    private static async Task<IResult> CreateNewUser(
        [FromServices] ISender sender,
        [FromServices] JwtService jwtService,
        [FromBody] CreateNewUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await sender.Send(
            new CreateFido2KeyAndUserRequest(request.Attestation, request.ChallengeContext, request.UserName),
            cancellationToken
        );
        var token = jwtService.GenerateToken(response.User);
        return Results.Json(new CreateNewUserResponse(token));
    }

    private static async Task<IResult> CreateExistingUser(
        [FromServices] ISender sender,
        [FromServices] JwtService jwtService,
        [FromBody] CreateExistingUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await sender.Send(
            new CreateFido2KeyRequest(request.Attestation, request.ChallengeContext),
            cancellationToken
        );
        var token = jwtService.GenerateToken(response.User);
        return Results.Json(new CreateExistingUserResponse(token));
    }

    private static async Task<IResult> GetAssertionChallenge(
        [FromServices] ISender sender,
        [FromQuery] string phoneNumber
    )
    {
        try
        {
            var response = await sender.Send(new CreateFido2AssertionRequest(new(phoneNumber)));
            return Results.Json(new GetAssertionChallengeResponse(response.Assertion, response.AssertionContext));
        }
        catch (PhoneNumberFormatException)
        {
            return Results.BadRequest();
        }
    }

    private static async Task<IResult> Assert(
        [FromServices] ISender sender,
        [FromServices] JwtService jwtService,
        [FromBody] AssertRequest request,
        CancellationToken cancellationToken
    )
    {
        var response = await sender.Send(
            new AssertFido2AssertionRequest(request.Assertion, request.Context),
            cancellationToken
        );
        var token = jwtService.GenerateToken(response.User);
        return Results.Json(new AssertResponse(token));
    }

    private static readonly Dictionary<string, Dictionary<string, (byte[], uint)>> Users = [];
}

[JsonSerializable(typeof(GetCreationChallengeResponse))]
[JsonSerializable(typeof(CreateNewUserRequest))]
[JsonSerializable(typeof(CreateNewUserResponse))]
[JsonSerializable(typeof(AssertRequest))]
[JsonSerializable(typeof(AssertResponse))]
public sealed partial class AuthSerializerContext : JsonSerializerContext;

public sealed record VerifyPhoneNumberResponse([property: JsonPropertyName("context")] string Context);

public sealed record GetCreationChallengeRequest(
    [property: JsonPropertyName("phoneNumberVerificationCode")] string PhoneNumberVerificationCode,
    [property: JsonPropertyName("phoneNumberVerificationContext")] string PhoneNumberVerificationContext
);

public sealed record GetCreationChallengeResponse(
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("options")] CredentialCreateOptions Options,
    [property: JsonPropertyName("userExists")] bool UserExists
);

public sealed record CreateNewUserRequest(
    [property: JsonPropertyName("attestation")] AuthenticatorAttestationRawResponse Attestation,
    [property: JsonPropertyName("challengeContext")] string ChallengeContext,
    [property: JsonPropertyName("userName")] string UserName
);

public sealed record CreateNewUserResponse([property: JsonPropertyName("token")] string Token);

public sealed record CreateExistingUserRequest(
    [property: JsonPropertyName("attestation")] AuthenticatorAttestationRawResponse Attestation,
    [property: JsonPropertyName("challengeContext")] string ChallengeContext
);

public sealed record CreateExistingUserResponse([property: JsonPropertyName("token")] string Token);

public sealed record GetAssertionChallengeResponse(
    [property: JsonPropertyName("assertion")] AssertionOptions Assertion,
    [property: JsonPropertyName("assertionContext")] string Context
);

public sealed record AssertRequest(
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("assertion")] AuthenticatorAssertionRawResponse Assertion
);

public sealed record AssertResponse([property: JsonPropertyName("token")] string Token);
