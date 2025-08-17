using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;

namespace Split.Domain.User.Events;

public sealed record CreateFido2KeyAndUserRequest(
    AuthenticatorAttestationRawResponse Attestation,
    string ChallengeContext,
    string UserName
) : IRequest<CreateFido2KeyAndUserResponse>;

public sealed record CreateFido2KeyAndUserResponse();

public record CreateFido2KeyAndUserRequestHandler(AuthService authService)
    : IRequestHandler<CreateFido2KeyAndUserRequest, CreateFido2KeyAndUserResponse>
{
    public async ValueTask<CreateFido2KeyAndUserResponse> Handle(
        CreateFido2KeyAndUserRequest request,
        CancellationToken cancellationToken
    )
    {
        await authService.CreateUserAndAuthKey(
            request.Attestation,
            request.ChallengeContext,
            new(request.UserName),
            cancellationToken
        );
        return new();
    }
}
