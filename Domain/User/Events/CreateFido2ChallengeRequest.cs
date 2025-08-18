using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;

namespace Split.Domain.User.Events;

public sealed record CreateFido2ChallengeRequest(
    string PhoneNumberVerificationCode,
    string PhoneNumberVerificationContext
) : IRequest<CreateFido2ChallengeResponse>;

public sealed record CreateFido2ChallengeResponse(CreateFido2ChallengeResult? Result);

public sealed record CreateFido2ChallengeResult(
    CredentialCreateOptions Challenge,
    string ChallengeContext,
    bool UserExists
);

public record CreateFido2ChallengeRequestHandler(AuthService authService)
    : IRequestHandler<CreateFido2ChallengeRequest, CreateFido2ChallengeResponse>
{
    public async ValueTask<CreateFido2ChallengeResponse> Handle(
        CreateFido2ChallengeRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.CreateChallenge(
            request.PhoneNumberVerificationCode,
            request.PhoneNumberVerificationContext,
            cancellationToken
        );
        return new(result is null ? null : new(result.Challenge, result.ChallengeContext, result.UserExists));
    }
}
